using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Models;
using backend.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

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
        private readonly MissionsController _missionsController;

        public TalesController(AppDbContext context, ILogger<TalesController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _weeklyLorePath = Path.Combine(env.ContentRootPath, "Data", "WeeklyLore.json");
            _dailyMissionsPath = Path.Combine(env.ContentRootPath, "Data", "DailyMissions.json");
            _missionsController = new MissionsController(context);
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
                
                // Combine progress and claimed missions (character progress takes precedence over user progress)
                var allProgress = new Dictionary<string, int>(userProgress);
                foreach (var kvp in characterProgress)
                {
                    allProgress[kvp.Key] = kvp.Value; // This will overwrite if key exists
                }
                
                var allClaimed = new Dictionary<string, DateTime>(userClaimed);
                foreach (var kvp in characterClaimed)
                {
                    allClaimed[kvp.Key] = kvp.Value; // This will overwrite if key exists
                }
                
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
                _logger.LogInformation("Claim mission request: MissionId={MissionId}, MissionType={MissionType}", 
                    request.MissionId, request.MissionType);

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User not authenticated for mission claim");
                    return Unauthorized("User not authenticated");
                }

                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null)
                {
                    _logger.LogWarning("User not found for mission claim: UserId={UserId}", userId);
                    return NotFound("User not found");
                }

                var character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId));

                var missionData = await GetMissionById(request.MissionId, request.MissionType);
                if (missionData == null)
                {
                    _logger.LogWarning("Mission not found: MissionId={MissionId}, MissionType={MissionType}", 
                        request.MissionId, request.MissionType);
                    return BadRequest("Mission not found");
                }

                var (rewardType, rewardAmount, rewardItem) = missionData.Value;

                // Check if mission is already claimed
                var claimedMissions = GetClaimedMissions(rewardType == "character" ? (object?)character ?? user : user, rewardType);
                var now = DateTime.UtcNow;
                var claimKey = GetClaimKey(request.MissionId, request.MissionType, now);
                
                if (claimedMissions.ContainsKey(claimKey))
                {
                    _logger.LogWarning("Mission already claimed: MissionId={MissionId}, ClaimKey={ClaimKey}", 
                        request.MissionId, claimKey);
                    return BadRequest("Mission already claimed");
                }

                // Check if mission is completed
                // For daily missions, check both user and character progress and use the higher value
                var userProgress = GetMissionProgress(user, "user");
                var characterProgress = character != null ? GetMissionProgress(character, "character") : new Dictionary<string, int>();
                
                var currentProgress = 0;
                if (request.MissionType == "daily")
                {
                    // For daily missions, use the higher progress between user and character
                    var userProgressValue = userProgress.GetValueOrDefault(request.MissionId, 0);
                    var characterProgressValue = characterProgress.GetValueOrDefault(request.MissionId, 0);
                    currentProgress = Math.Max(userProgressValue, characterProgressValue);
                }
                else
                {
                    // For weekly missions, use the appropriate progress based on reward type
                    var missionProgress = GetMissionProgress(rewardType == "character" ? (object?)character ?? user : user, rewardType);
                    currentProgress = missionProgress.GetValueOrDefault(request.MissionId, 0);
                }
                
                var requiredProgress = await GetRequiredProgress(request.MissionId, request.MissionType);
                
                
                if (currentProgress < requiredProgress)
                {
                    _logger.LogWarning("Mission not completed: MissionId={MissionId}, Current={Current}, Required={Required}", 
                        request.MissionId, currentProgress, requiredProgress);
                    return BadRequest("Mission not completed yet");
                }

                // Apply rewards
                if (rewardType == "character" && character != null)
                {
                    if (rewardItem == "xp")
                    {
                        character.ExperiencePoints += rewardAmount;
                        
                        // Check for level up using the same logic as BattleController
                        while (character.ExperiencePoints >= character.MaxExperiencePoints)
                        {
                            character.ExperiencePoints -= character.MaxExperiencePoints;
                            character.Level += 1;
                            character.UnspentStatPoints += 5; // 5 stat points per level (same as BattleController)
                            character.MaxExperiencePoints = (int)(character.MaxExperiencePoints * 1.1); // 10% increase per level
                            character.CurrentHealth = character.MaxHealth; // Full heal on level up
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

                // Prepare response with character data if it's a character reward
                var response = new MissionClaimResponse
                {
                    Success = true,
                    Message = "Mission claimed successfully!",
                    RewardAmount = rewardAmount,
                    RewardType = rewardType,
                    RewardItem = rewardItem
                };

                // Include character data if it's a character reward
                if (rewardType == "character" && character != null)
                {
                    response.Character = new
                    {
                        id = character.Id,
                        name = character.Name,
                        level = character.Level,
                        experiencePoints = character.ExperiencePoints,
                        maxExperiencePoints = character.MaxExperiencePoints,
                        currentHealth = character.CurrentHealth,
                        maxHealth = character.MaxHealth,
                        currentEnergy = character.CurrentEnergy,
                        maxEnergy = character.MaxEnergy,
                        attack = character.Attack,
                        defense = character.Defense,
                        agility = character.Agility,
                        magic = character.Magic,
                        speed = character.Speed,
                        criticalChance = character.CriticalChance,
                        credits = character.Credits,
                        unspentStatPoints = character.UnspentStatPoints,
                        canAllocateStats = character.UnspentStatPoints >= 5,
                        characterClass = character.Class,
                        profileIconUrl = character.ProfileIconUrl,
                        createdAt = character.CreatedAt
                    };
                }
                
                // Include user data if it's a user reward
                if (rewardType == "user")
                {
                    response.User = new
                    {
                        id = user.Id,
                        username = user.Username,
                        level = user.Level,
                        experiencePoints = user.ExperiencePoints,
                        maxExperiencePoints = user.MaxExperiencePoints,
                        credits = user.Credits,
                        createdAt = user.CreatedAt
                    };
                }

                return Ok(response);
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

        private async Task<bool> IsMissionCompleted(string missionId, string missionType, Dictionary<string, int> progress)
        {
            if (!progress.ContainsKey(missionId))
                return false;
                
            var currentProgress = progress[missionId];
            
            // Define completion thresholds based on mission type and ID
            var requiredProgress = await GetRequiredProgress(missionId, missionType);
            return currentProgress >= requiredProgress;
        }

        private async Task<int> GetRequiredProgress(string missionId, string missionType)
        {
            try
            {
                if (missionType == "daily")
                {
                    var dailyMissions = await GetDailyMissions();
                    var mission = dailyMissions.FirstOrDefault(m => m.Id == missionId);
                    if (mission != null)
                    {
                        return ExtractNumberFromDescription(mission.Description);
                    }
                }
                else if (missionType == "weekly")
                {
                    var weeklyLore = await GetCurrentWeeklyLore();
                    var mission = weeklyLore.WeeklyMissions.FirstOrDefault(m => m.Id == missionId);
                    if (mission != null)
                    {
                        return ExtractNumberFromDescription(mission.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting required progress for mission {MissionId}", missionId);
            }
            
            // Fallback to basic thresholds if extraction fails
            return missionType == "daily" ? 10 : 20;
        }

        private int ExtractNumberFromDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return 10;
            
            // Extract number from patterns like "Earn 100 credits", "Play 5 battles", etc.
            var match = System.Text.RegularExpressions.Regex.Match(description, @"(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
            {
                return number;
            }
            
            return 10; // Default fallback
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


        private DateTime GetLastResetDate(Character character)
        {
            try
            {
                if (string.IsNullOrEmpty(character.LastDailyResetJson))
                    return DateTime.MinValue;
                
                var lastReset = JsonSerializer.Deserialize<DateTime>(character.LastDailyResetJson);
                return lastReset;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private void SetLastResetDate(Character character, DateTime date, string resetType)
        {
            try
            {
                if (resetType == "daily")
                {
                    character.LastDailyResetJson = JsonSerializer.Serialize(date);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting last reset date for character {CharacterId}", character.Id);
            }
        }

        private int GetLastWeeklyResetWeek(Character character)
        {
            try
            {
                if (string.IsNullOrEmpty(character.LastWeeklyResetJson))
                    return 0;
                
                var lastResetWeek = JsonSerializer.Deserialize<int>(character.LastWeeklyResetJson);
                return lastResetWeek;
            }
            catch
            {
                return 0;
            }
        }

        private void SetLastWeeklyResetWeek(Character character, int weekNumber)
        {
            try
            {
                character.LastWeeklyResetJson = JsonSerializer.Serialize(weekNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting last weekly reset week for character {CharacterId}", character.Id);
            }
        }

        [HttpPost("reset-daily-missions")]
        [Authorize]
        public async Task<ActionResult> ResetDailyMissions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId));

                if (character == null)
                {
                    return NotFound("Character not found");
                }

                // Check if we need to reset daily missions
                var now = DateTime.UtcNow;
                var lastResetDate = GetLastResetDate(character);
                var today = now.Date;
                
                if (lastResetDate.Date < today)
                {
                    await _missionsController.ResetMissionProgress(character, "daily");
                    SetLastResetDate(character, today, "daily");
                    await _context.SaveChangesAsync();
                    
                    return Ok(new { message = "Daily missions reset successfully" });
                }
                
                return Ok(new { message = "Daily missions already reset for today" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting daily missions");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("reset-weekly-missions")]
        [Authorize]
        public async Task<ActionResult> ResetWeeklyMissions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId));

                if (character == null)
                {
                    return NotFound("Character not found");
                }

                // Check if we need to reset weekly missions
                var now = DateTime.UtcNow;
                var currentWeekNumber = GetWeekNumber(now);
                var lastWeeklyResetWeek = GetLastWeeklyResetWeek(character);
                
                if (lastWeeklyResetWeek < currentWeekNumber)
                {
                    await _missionsController.ResetMissionProgress(character, "weekly");
                    SetLastWeeklyResetWeek(character, currentWeekNumber);
                    await _context.SaveChangesAsync();
                    
                    return Ok(new { message = "Weekly missions reset successfully" });
                }
                
                return Ok(new { message = "Weekly missions already reset for this week" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting weekly missions");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
