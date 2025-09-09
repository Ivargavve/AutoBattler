using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Models;
using backend.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TalesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TalesController> _logger;
        private readonly string _weeklyLorePath;
        private readonly string _dailyMissionsPath;

        public TalesController(AppDbContext context, ILogger<TalesController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _weeklyLorePath = Path.Combine(env.ContentRootPath, "Data", "WeeklyLore.json");
            _dailyMissionsPath = Path.Combine(env.ContentRootPath, "Data", "DailyMissions.json");
        }

        [HttpGet]
        public async Task<ActionResult<TalesResponse>> GetTales()
        {
            try
            {
                var weeklyLore = await GetCurrentWeeklyLore();
                var dailyMissions = await GetDailyMissions();
                
                // Calculate next reset times (global for all users)
                var now = DateTime.UtcNow;
                var nextDailyReset = now.Date.AddDays(1); // Next day at midnight UTC
                var nextWeeklyReset = GetNextWeeklyReset(now);
                
                return Ok(new TalesResponse
                {
                    CurrentLore = weeklyLore,
                    DailyMissions = dailyMissions,
                    WeeklyMissions = weeklyLore.WeeklyMissions,
                    LastUpdated = now,
                    NextDailyReset = nextDailyReset,
                    NextWeeklyReset = nextWeeklyReset,
                    MissionProgress = new Dictionary<string, int>(),
                    ClaimedMissions = new Dictionary<string, DateTime>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tales data");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user-missions")]
        [Authorize]
        public async Task<ActionResult<TalesResponse>> GetUserMissions()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId));

                // Get global weekly lore and daily missions (same for all users)
                var weeklyLore = await GetCurrentWeeklyLore();
                var dailyMissions = await GetDailyMissions();
                
                // Calculate next reset times (global for all users)
                var now = DateTime.UtcNow;
                var nextDailyReset = now.Date.AddDays(1);
                var nextWeeklyReset = GetNextWeeklyReset(now);
                
                // Get user's mission progress and claimed missions
                var userProgress = GetMissionProgress(user, "user");
                var userClaimed = GetClaimedMissions(user, "user");
                var characterProgress = character != null ? GetMissionProgress(character, "character") : new Dictionary<string, int>();
                var characterClaimed = character != null ? GetClaimedMissions(character, "character") : new Dictionary<string, DateTime>();
                
                // Combine progress and claimed missions
                var allProgress = userProgress.Concat(characterProgress).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var allClaimed = userClaimed.Concat(characterClaimed).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                
                return Ok(new TalesResponse
                {
                    CurrentLore = weeklyLore,
                    DailyMissions = dailyMissions,
                    WeeklyMissions = weeklyLore.WeeklyMissions,
                    LastUpdated = now,
                    NextDailyReset = nextDailyReset,
                    NextWeeklyReset = nextWeeklyReset,
                    MissionProgress = allProgress,
                    ClaimedMissions = allClaimed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user missions");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("claim-mission")]
        [Authorize]
        public async Task<ActionResult<MissionClaimResponse>> ClaimMission([FromBody] MissionClaimRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId));

                var missionData = await GetMissionById(request.MissionId, request.MissionType);
                if (missionData == null)
                {
                    return BadRequest("Mission not found");
                }

                var (rewardType, rewardAmount, rewardItem) = missionData.Value;

                // Check if mission is already claimed
                var claimedMissions = GetClaimedMissions(rewardType == "character" ? (object?)character ?? user : user, rewardType);
                var now = DateTime.UtcNow;
                var claimKey = GetClaimKey(request.MissionId, request.MissionType, now);
                
                if (claimedMissions.ContainsKey(claimKey))
                {
                    return BadRequest("Mission already claimed");
                }

                // Check if mission is completed
                var missionProgress = GetMissionProgress(rewardType == "character" ? (object?)character ?? user : user, rewardType);
                if (!IsMissionCompleted(request.MissionId, request.MissionType, missionProgress))
                {
                    return BadRequest("Mission not completed yet");
                }

                // Apply rewards
                if (rewardType == "character" && character != null)
                {
                    if (rewardItem == "xp")
                    {
                        character.ExperiencePoints += rewardAmount;
                        // Check for level up
                        var newLevel = CalculateLevel(character.ExperiencePoints);
                        if (newLevel > character.Level)
                        {
                            var oldLevel = character.Level;
                            character.Level = newLevel;
                            character.UnspentStatPoints += (newLevel - oldLevel) * 2; // 2 stat points per level
                        }
                    }

                    // Mark mission as claimed
                    claimedMissions[claimKey] = now;
                    character.ClaimedMissionsJson = JsonSerializer.Serialize(claimedMissions);
                }
                else if (rewardType == "user")
                {
                    if (rewardItem == "credits")
                    {
                        user.Credits += rewardAmount;
                    }

                    // Mark mission as claimed
                    claimedMissions[claimKey] = now;
                    user.ClaimedMissionsJson = JsonSerializer.Serialize(claimedMissions);
                }

                await _context.SaveChangesAsync();

                return Ok(new MissionClaimResponse
                {
                    Success = true,
                    Message = "Mission claimed successfully!",
                    RewardAmount = rewardAmount,
                    RewardType = rewardType,
                    RewardItem = rewardItem
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error claiming mission");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("update-progress")]
        [Authorize]
        public async Task<ActionResult> UpdateMissionProgress([FromBody] MissionProgressUpdateRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Update character progress if applicable
                if (request.CharacterId.HasValue)
                {
                    var character = await _context.Characters
                        .FirstOrDefaultAsync(c => c.Id == request.CharacterId.Value && c.UserId == int.Parse(userId));
                    
                    if (character != null)
                    {
                        var progress = GetMissionProgress(character, "character");
                        progress[request.MissionId] = request.Progress;
                        character.MissionProgressJson = JsonSerializer.Serialize(progress);
                    }
                }
                else
                {
                    // Update user progress
                    var progress = GetMissionProgress(user, "user");
                    progress[request.MissionId] = request.Progress;
                    user.MissionProgressJson = JsonSerializer.Serialize(progress);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mission progress");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<WeeklyLore> GetCurrentWeeklyLore()
        {
            try
            {
                var jsonContent = await System.IO.File.ReadAllTextAsync(_weeklyLorePath);
                var weeklyLoreList = JsonSerializer.Deserialize<List<WeeklyLore>>(jsonContent);
                
                if (weeklyLoreList == null || !weeklyLoreList.Any())
                {
                    throw new InvalidOperationException("No weekly lore data found");
                }

                // Get current week's lore using ISO week number for consistency
                var now = DateTime.UtcNow;
                var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
                var weekNumber = calendar.GetWeekOfYear(now, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                var loreIndex = (weekNumber - 1) % weeklyLoreList.Count;
                
                return weeklyLoreList[loreIndex];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading weekly lore");
                throw;
            }
        }

        private async Task<List<DailyMission>> GetDailyMissions()
        {
            try
            {
                var jsonContent = await System.IO.File.ReadAllTextAsync(_dailyMissionsPath);
                var dailyMissions = JsonSerializer.Deserialize<List<DailyMission>>(jsonContent);
                
                if (dailyMissions == null || !dailyMissions.Any())
                {
                    throw new InvalidOperationException("No daily missions data found");
                }

                // Use date-based selection for consistent daily missions across all users
                var today = DateTime.UtcNow.Date;
                var dayOfYear = today.DayOfYear;
                var random = new Random(dayOfYear); // Use day of year as seed for consistency
                
                return dailyMissions.OrderBy(x => random.Next()).Take(3).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading daily missions");
                throw;
            }
        }

        private async Task<(string RewardType, int RewardAmount, string RewardItem)?> GetMissionById(string missionId, string missionType)
        {
            try
            {
                if (missionType == "daily")
                {
                    var jsonContent = await System.IO.File.ReadAllTextAsync(_dailyMissionsPath);
                    var dailyMissions = JsonSerializer.Deserialize<List<DailyMission>>(jsonContent);
                    var mission = dailyMissions?.FirstOrDefault(m => m.Id == missionId);
                    if (mission != null)
                    {
                        return (mission.RewardType, mission.RewardAmount, mission.RewardItem);
                    }
                }
                else if (missionType == "weekly")
                {
                    var weeklyLore = await GetCurrentWeeklyLore();
                    var mission = weeklyLore.WeeklyMissions.FirstOrDefault(m => m.Id == missionId);
                    if (mission != null)
                    {
                        return (mission.RewardType, mission.RewardAmount, mission.RewardItem);
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mission by ID");
                return null;
            }
        }

        private int CalculateLevel(int xp)
        {
            // Simple level calculation: 100 XP per level
            return (xp / 100) + 1;
        }

        private Dictionary<string, int> GetMissionProgress(object entity, string type)
        {
            try
            {
                string json = type == "character" 
                    ? ((Character)entity).MissionProgressJson 
                    : ((User)entity).MissionProgressJson;
                
                if (string.IsNullOrEmpty(json) || json == "{}")
                    return new Dictionary<string, int>();
                
                return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }

        private Dictionary<string, DateTime> GetClaimedMissions(object entity, string type)
        {
            try
            {
                string json = type == "character" 
                    ? ((Character)entity).ClaimedMissionsJson 
                    : ((User)entity).ClaimedMissionsJson;
                
                if (string.IsNullOrEmpty(json) || json == "{}")
                    return new Dictionary<string, DateTime>();
                
                return JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json) ?? new Dictionary<string, DateTime>();
            }
            catch
            {
                return new Dictionary<string, DateTime>();
            }
        }

        private bool IsMissionCompleted(string missionId, string missionType, Dictionary<string, int> progress)
        {
            // This is a simplified version - in reality you'd check against mission requirements
            // For now, we'll assume missions are completed if they have progress > 0
            if (!progress.ContainsKey(missionId))
                return false;
                
            var currentProgress = progress[missionId];
            
            // Define completion thresholds based on mission type and ID
            var requiredProgress = GetRequiredProgress(missionId, missionType);
            return currentProgress >= requiredProgress;
        }

        private int GetRequiredProgress(string missionId, string missionType)
        {
            // Define specific progress requirements for each mission
            // This is a simplified version - in reality you'd have more complex logic
            return missionType == "daily" ? 1 : 1; // For now, all missions require 1 progress point
        }

        private string GetClaimKey(string missionId, string missionType, DateTime now)
        {
            if (missionType == "daily")
            {
                return $"{missionId}_{now:yyyyMMdd}";
            }
            else // weekly
            {
                var weekNumber = GetWeekNumber(now);
                return $"{missionId}_week_{weekNumber}";
            }
        }

        private int GetWeekNumber(DateTime date)
        {
            // Get ISO week number
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        private DateTime GetNextWeeklyReset(DateTime now)
        {
            // Reset weekly missions on Monday at midnight UTC
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0 && now.Hour == 0 && now.Minute == 0)
            {
                daysUntilMonday = 7; // If it's exactly Monday midnight, next reset is next Monday
            }
            return now.Date.AddDays(daysUntilMonday);
        }

    }
}
