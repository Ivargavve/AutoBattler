using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using backend.Models;
using System.Text.Json;
using System.Linq; 
using System;
using System.Collections.Generic;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MissionsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MissionsController(AppDbContext db)
        {
            _db = db;
        }

        public async Task UpdateEnemyTypeDefeatMissionProgress(Character player, string enemyType)
        {
            try
            {
                if (player.User == null) return;

                var userProgress = GetMissionProgress(player.User, "user");
                var characterProgress = GetMissionProgress(player, "character");

                var dailyMissions = await GetDailyMissions();
                var weeklyLore = await GetCurrentWeeklyLore();
                var weeklyMissions = weeklyLore.WeeklyMissions;

                foreach (var mission in dailyMissions)
                {
                    if (IsEnemyTypeDefeatMission(mission.Description, enemyType))
                    {
                        var currentProgress = userProgress.GetValueOrDefault(mission.Id, 0);
                        var newProgress = currentProgress + 1;
                        var maxRequired = ExtractNumberFromDescription(mission.Description);
                        userProgress[mission.Id] = CapProgressAtMaximum(newProgress, maxRequired);
                    }
                }

                foreach (var mission in weeklyMissions)
                {
                    if (IsEnemyTypeDefeatMission(mission.Description, enemyType))
                    {
                        var currentProgress = characterProgress.GetValueOrDefault(mission.Id, 0);
                        var newProgress = currentProgress + 1;
                        var maxRequired = ExtractNumberFromDescription(mission.Description);
                        characterProgress[mission.Id] = CapProgressAtMaximum(newProgress, maxRequired);
                    }
                }

                player.User.MissionProgressJson = JsonSerializer.Serialize(userProgress);
                player.MissionProgressJson = JsonSerializer.Serialize(characterProgress);
            }
            catch (Exception){}
        }

        public async Task UpdateCreditMissionProgress(Character player, int gainedCredits)
        {
            try
            {
                if (player.User == null) return;

                var userProgress = GetMissionProgress(player.User, "user");
                var characterProgress = GetMissionProgress(player, "character");

                var dailyMissions = await GetDailyMissions();
                var weeklyLore = await GetCurrentWeeklyLore();
                var weeklyMissions = weeklyLore.WeeklyMissions;

                foreach (var mission in dailyMissions)
                {
                    if (IsCreditMission(mission.Description))
                    {
                        var currentProgress = userProgress.GetValueOrDefault(mission.Id, 0);
                        var newProgress = currentProgress + gainedCredits;
                        var maxRequired = ExtractNumberFromDescription(mission.Description);
                        userProgress[mission.Id] = CapProgressAtMaximum(newProgress, maxRequired);
                    }
                }

                foreach (var mission in weeklyMissions)
                {
                    if (IsCreditMission(mission.Description))
                    {
                        var currentProgress = characterProgress.GetValueOrDefault(mission.Id, 0);
                        var newProgress = currentProgress + gainedCredits;
                        var maxRequired = ExtractNumberFromDescription(mission.Description);
                        characterProgress[mission.Id] = CapProgressAtMaximum(newProgress, maxRequired);
                    }
                }

                player.User.MissionProgressJson = JsonSerializer.Serialize(userProgress);
                player.MissionProgressJson = JsonSerializer.Serialize(characterProgress);
            }
            catch (Exception){}
        }

        public async Task UpdateBattleCountMissionProgress(Character player)
        {
            try
            {
                if (player.User == null) return;

                var userProgress = GetMissionProgress(player.User, "user");
                var characterProgress = GetMissionProgress(player, "character");

                var dailyMissions = await GetDailyMissions();
                var weeklyLore = await GetCurrentWeeklyLore();
                var weeklyMissions = weeklyLore.WeeklyMissions;

                foreach (var mission in dailyMissions)
                {
                    if (IsBattleCountMission(mission.Description))
                    {
                        var currentProgress = userProgress.GetValueOrDefault(mission.Id, 0);
                        var newProgress = currentProgress + 1;
                        var maxRequired = ExtractNumberFromDescription(mission.Description);
                        userProgress[mission.Id] = CapProgressAtMaximum(newProgress, maxRequired);
                    }
                }

                foreach (var mission in weeklyMissions)
                {
                    if (IsBattleCountMission(mission.Description))
                    {
                        var currentProgress = characterProgress.GetValueOrDefault(mission.Id, 0);
                        var newProgress = currentProgress + 1;
                        var maxRequired = ExtractNumberFromDescription(mission.Description);
                        characterProgress[mission.Id] = CapProgressAtMaximum(newProgress, maxRequired);
                    }
                }

                player.User.MissionProgressJson = JsonSerializer.Serialize(userProgress);
                player.MissionProgressJson = JsonSerializer.Serialize(characterProgress);
            }
            catch (Exception){}
        }

        public async Task UpdateCriticalHitMissionProgress(Character player)
        {
            try
            {
                if (player.User == null) return;

                var userProgress = GetMissionProgress(player.User, "user");
                var characterProgress = GetMissionProgress(player, "character");

                var dailyMissions = await GetDailyMissions();
                var weeklyLore = await GetCurrentWeeklyLore();
                var weeklyMissions = weeklyLore.WeeklyMissions;

                foreach (var mission in dailyMissions)
                {
                    if (IsCriticalHitMission(mission.Description))
                    {
                        var currentProgress = userProgress.GetValueOrDefault(mission.Id, 0);
                        var newProgress = currentProgress + 1;
                        var maxRequired = ExtractNumberFromDescription(mission.Description);
                        userProgress[mission.Id] = CapProgressAtMaximum(newProgress, maxRequired);
                    }
                }

                foreach (var mission in weeklyMissions)
                {
                    if (IsCriticalHitMission(mission.Description))
                    {
                        var currentProgress = characterProgress.GetValueOrDefault(mission.Id, 0);
                        var newProgress = currentProgress + 1;
                        var maxRequired = ExtractNumberFromDescription(mission.Description);
                        characterProgress[mission.Id] = CapProgressAtMaximum(newProgress, maxRequired);
                    }
                }

                player.User.MissionProgressJson = JsonSerializer.Serialize(userProgress);
                player.MissionProgressJson = JsonSerializer.Serialize(characterProgress);
            }
            catch (Exception){}
        }

        [NonAction]
        public async Task UpdateAttackTypeMissionProgress(Character player, PlayerAttack attack)
        {
            try
            {
                if (player.User == null) return;

                var userProgress = GetMissionProgress(player.User, "user");
                var characterProgress = GetMissionProgress(player, "character");

                var dailyMissions = await GetDailyMissions();
                var weeklyLore = await GetCurrentWeeklyLore();
                var weeklyMissions = weeklyLore.WeeklyMissions;

                void UpdateProgress(Dictionary<string, int> progressDict, string missionId, int maxRequired)
                {
                    var currentProgress = progressDict.GetValueOrDefault(missionId, 0);
                    var newProgress = currentProgress + 1;
                    progressDict[missionId] = CapProgressAtMaximum(newProgress, maxRequired);
                }

                foreach (var mission in dailyMissions)
                {
                    var maxRequired = ExtractNumberFromDescription(mission.Description);
                    
                    if (IsPhysicalAttackMission(mission.Description) && IsPhysicalAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsMagicAttackMission(mission.Description) && IsMagicAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsHolyAttackMission(mission.Description) && IsHolyAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsRogueAbilityMission(mission.Description) && IsRogueAbility(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsElementalAttackMission(mission.Description) && IsElementalAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsDefensiveAbilityMission(mission.Description) && IsDefensiveAbility(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsAgilityBasedAttackMission(mission.Description) && IsAgilityBasedAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsAttackBasedAttackMission(mission.Description) && IsAttackBasedAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsMagicBasedAttackMission(mission.Description) && IsMagicBasedAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsDefenseBasedAttackMission(mission.Description) && IsDefenseBasedAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                }

                foreach (var mission in weeklyMissions)
                {
                    var maxRequired = ExtractNumberFromDescription(mission.Description);
                    
                    if (IsPhysicalAttackMission(mission.Description) && IsPhysicalAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsMagicAttackMission(mission.Description) && IsMagicAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsHolyAttackMission(mission.Description) && IsHolyAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsRogueAbilityMission(mission.Description) && IsRogueAbility(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsElementalAttackMission(mission.Description) && IsElementalAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsDefensiveAbilityMission(mission.Description) && IsDefensiveAbility(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    // New stat-based mission checks for weekly missions
                    else if (IsAgilityBasedAttackMission(mission.Description) && IsAgilityBasedAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsAttackBasedAttackMission(mission.Description) && IsAttackBasedAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsMagicBasedAttackMission(mission.Description) && IsMagicBasedAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                    else if (IsDefenseBasedAttackMission(mission.Description) && IsDefenseBasedAttack(attack))
                    {
                        if (mission.RewardType == "character")
                            UpdateProgress(characterProgress, mission.Id, maxRequired);
                        else
                            UpdateProgress(userProgress, mission.Id, maxRequired);
                    }
                }

                player.User.MissionProgressJson = JsonSerializer.Serialize(userProgress);
                player.MissionProgressJson = JsonSerializer.Serialize(characterProgress);
            }
            catch (Exception){}
        }

        private bool IsEnemyTypeDefeatMission(string description, string enemyType)
        {
            if (string.IsNullOrEmpty(description)) return false;
            
            var typePattern = enemyType.ToLower();
            var pattern = $@"defeat\s+\d+\s+{typePattern}(-type)?\s+enemies?";
            
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                pattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsCreditMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"(earn|gain|collect|get)\s+\d+\s+credits?", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
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

        private async Task<List<DailyMission>> GetDailyMissions()
        {
            try
            {
                var dailyMissionsPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "DailyMissions.json");
                var jsonContent = await System.IO.File.ReadAllTextAsync(dailyMissionsPath);
                var dailyMissions = JsonSerializer.Deserialize<List<DailyMission>>(jsonContent);
                return dailyMissions ?? new List<DailyMission>();
            }
            catch
            {
                return new List<DailyMission>();
            }
        }

        private async Task<WeeklyLore> GetCurrentWeeklyLore()
        {
            try
            {
                var weeklyLorePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "WeeklyLore.json");
                var jsonContent = await System.IO.File.ReadAllTextAsync(weeklyLorePath);
                var weeklyLoreList = JsonSerializer.Deserialize<List<WeeklyLore>>(jsonContent);
                
                if (weeklyLoreList == null || !weeklyLoreList.Any())
                    return new WeeklyLore();

                var now = DateTime.UtcNow;
                var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
                var weekNumber = calendar.GetWeekOfYear(now, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                var loreIndex = (weekNumber - 1) % weeklyLoreList.Count;
                
                return weeklyLoreList[loreIndex];
            }
            catch
            {
                return new WeeklyLore();
            }
        }

        private bool IsBattleCountMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"play\s+\d+\s+battles?", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsCriticalHitMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"(get|score|hit)\s+\d+\s+critical\s+hits?", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsPhysicalAttackMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"use\s+\d+\s+physical\s+attacks?", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsMagicAttackMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"use\s+\d+\s+magic\s+attacks?", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsHolyAttackMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"use\s+\d+\s+holy\s+attacks?", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsRogueAbilityMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"use\s+\d+\s+(rogue\s+abilities?|poison\s+attacks?)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsElementalAttackMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"use\s+(fire|ice|arcane)\s+attacks?\s+\d+\s+times?", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsDefensiveAbilityMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"use\s+\d+\s+defensive\s+abilities?", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsAgilityBasedAttackMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"use\s+\d+\s+(agility[-\s]?based\s+(attacks?|abilities?)|agility\s+(attacks?|abilities?)|poison\s+attacks?)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsAttackBasedAttackMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"use\s+\d+\s+(attack[-\s]?based\s+(attacks?|abilities?)|attack\s+(attacks?|abilities?))", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsMagicBasedAttackMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"use\s+\d+\s+(magic[-\s]?based\s+(attacks?|abilities?)|magic\s+(attacks?|abilities?))", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsDefenseBasedAttackMission(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(description, 
                @"use\s+\d+\s+(defense[-\s]?based\s+(attacks?|abilities?)|defense\s+(attacks?|abilities?))", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsPhysicalAttack(PlayerAttack attack)
        {
            if (attack == null || string.IsNullOrEmpty(attack.Name)) return false;
            var name = attack.Name.ToLower();
            return name.Contains("slash") || name.Contains("strike") || name.Contains("punch") || 
                   name.Contains("kick") || name.Contains("bash") || name.Contains("smash") ||
                   name.Contains("cleave") || name.Contains("smite") || name.Contains("quick") ||
                   name.Contains("shot") || name.Contains("backstab");
        }

        private bool IsMagicAttack(PlayerAttack attack)
        {
            if (attack == null || string.IsNullOrEmpty(attack.Name)) return false;
            var name = attack.Name.ToLower();
            return name.Contains("magic") || name.Contains("spell") || name.Contains("arcane") ||
                   name.Contains("mystic") || name.Contains("enchant") || name.Contains("fireball") ||
                   name.Contains("ice") || name.Contains("frost") || name.Contains("blizzard") ||
                   name.Contains("flame") || name.Contains("bolt") || name.Contains("blast");
        }

        private bool IsHolyAttack(PlayerAttack attack)
        {
            if (attack == null || string.IsNullOrEmpty(attack.Name)) return false;
            var name = attack.Name.ToLower();
            return name.Contains("holy") || name.Contains("divine") || name.Contains("blessed") ||
                   name.Contains("sacred") || name.Contains("light") || name.Contains("smite") ||
                   name.Contains("judgement") || name.Contains("holy light");
        }

        private bool IsRogueAbility(PlayerAttack attack)
        {
            if (attack == null || string.IsNullOrEmpty(attack.Name)) return false;
            var name = attack.Name.ToLower();
            return name.Contains("stealth") || name.Contains("sneak") || name.Contains("backstab") ||
                   name.Contains("shadow") || name.Contains("dagger") || name.Contains("trick") ||
                   name.Contains("shadowstep") || name.Contains("poison") || name.Contains("quick") ||
                   name.Contains("shot") || name.Contains("camouflage") || name.Contains("nature's grasp");
        }

        private bool IsElementalAttack(PlayerAttack attack)
        {
            if (attack == null || string.IsNullOrEmpty(attack.Name)) return false;
            var name = attack.Name.ToLower();
            return name.Contains("fire") || name.Contains("ice") || name.Contains("frost") ||
                   name.Contains("arcane") || name.Contains("flame") || name.Contains("blizzard") ||
                   name.Contains("fireball") || name.Contains("bolt") || name.Contains("blast");
        }

        private bool IsDefensiveAbility(PlayerAttack attack)
        {
            if (attack == null || string.IsNullOrEmpty(attack.Name)) return false;
            var name = attack.Name.ToLower();
            return name.Contains("block") || name.Contains("shield") || name.Contains("defend") ||
                   name.Contains("guard") || name.Contains("protect") || name.Contains("barrier") ||
                   name.Contains("sacred shield") || name.Contains("mana shield");
        }

        private bool IsAgilityBasedAttack(PlayerAttack attack)
        {
            if (attack == null) return false;
            
            if (attack.RequiredStats != null && attack.RequiredStats.ContainsKey("agility"))
            {
                return true;
            }
            
            var name = attack.Name.ToLower();
            return name.Contains("quick") || name.Contains("shadow") || name.Contains("poison") ||
                   name.Contains("shot") || name.Contains("step") || name.Contains("sneak") ||
                   name.Contains("backstab") || name.Contains("camouflage") || name.Contains("nature's grasp");
        }

        private bool IsAttackBasedAttack(PlayerAttack attack)
        {
            if (attack == null) return false;
            
            if (attack.RequiredStats != null && attack.RequiredStats.ContainsKey("attack"))
            {
                return true;
            }
            
            var name = attack.Name.ToLower();
            return name.Contains("slash") || name.Contains("strike") || name.Contains("punch") ||
                   name.Contains("kick") || name.Contains("bash") || name.Contains("smash") ||
                   name.Contains("cleave") || name.Contains("battle shout");
        }

        private bool IsMagicBasedAttack(PlayerAttack attack)
        {
            if (attack == null) return false;
            
            if (attack.RequiredStats != null && attack.RequiredStats.ContainsKey("magic"))
            {
                return true;
            }
            
            var name = attack.Name.ToLower();
            return name.Contains("magic") || name.Contains("spell") || name.Contains("arcane") ||
                   name.Contains("mystic") || name.Contains("enchant") || name.Contains("fireball") ||
                   name.Contains("ice") || name.Contains("frost") || name.Contains("holy") ||
                   name.Contains("divine") || name.Contains("blessed") || name.Contains("sacred");
        }

        private bool IsDefenseBasedAttack(PlayerAttack attack)
        {
            if (attack == null) return false;
            
            if (attack.RequiredStats != null && attack.RequiredStats.ContainsKey("defense"))
            {
                return true;
            }
            
            var name = attack.Name.ToLower();
            return name.Contains("block") || name.Contains("shield") || name.Contains("defend") ||
                   name.Contains("guard") || name.Contains("protect") || name.Contains("barrier");
        }

        private int ExtractNumberFromDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return 10;
            
            var match = System.Text.RegularExpressions.Regex.Match(description, @"(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
            {
                return number;
            }
            
            return 10;
        }

        private int CapProgressAtMaximum(int currentProgress, int maxRequired)
        {
            return Math.Min(currentProgress, maxRequired);
        }

        public async Task ResetMissionProgress(Character character, string resetType)
        {
            try
            {
                if (character.User == null) return;

                if (resetType == "daily")
                {
                    await ResetDailyMissionProgress(character);
                }
                else if (resetType == "weekly")
                {
                    await ResetWeeklyMissionProgress(character);
                }
            }
            catch (Exception){}
        }

        private async Task ResetDailyMissionProgress(Character character)
        {
           
            var currentDailyMissions = await GetDailyMissions();
            var characterProgress = GetMissionProgress(character, "character");
            var userProgress = GetMissionProgress(character.User!, "user");
            
            bool shouldResetCharacter = false;
            bool shouldResetUser = false;
            
            foreach (var mission in currentDailyMissions)
            {
                if (!characterProgress.ContainsKey(mission.Id))
                {
                    shouldResetCharacter = true;
                    break;
                }
            }
            
            foreach (var mission in currentDailyMissions)
            {
                if (!userProgress.ContainsKey(mission.Id))
                {
                    shouldResetUser = true;
                    break;
                }
            }

            if (shouldResetCharacter)
            {
                foreach (var mission in currentDailyMissions)
                {
                    if (characterProgress.ContainsKey(mission.Id))
                    {
                        characterProgress.Remove(mission.Id);
                    }
                }
                character.MissionProgressJson = JsonSerializer.Serialize(characterProgress);
            }

            if (shouldResetUser)
            {
                foreach (var mission in currentDailyMissions)
                {
                    if (userProgress.ContainsKey(mission.Id))
                    {
                        userProgress.Remove(mission.Id);
                    }
                }
                character.User!.MissionProgressJson = JsonSerializer.Serialize(userProgress);
            }
        }

        private async Task ResetWeeklyMissionProgress(Character character)
        {
            var currentWeeklyLore = await GetCurrentWeeklyLore();
            var characterProgress = GetMissionProgress(character, "character");
            var userProgress = GetMissionProgress(character.User!, "user");
            
            bool shouldResetCharacter = false;
            bool shouldResetUser = false;
            
            foreach (var mission in currentWeeklyLore.WeeklyMissions)
            {
                if (!characterProgress.ContainsKey(mission.Id))
                {
                    shouldResetCharacter = true;
                    break;
                }
            }
            
            foreach (var mission in currentWeeklyLore.WeeklyMissions)
            {
                if (!userProgress.ContainsKey(mission.Id))
                {
                    shouldResetUser = true;
                    break;
                }
            }

            if (shouldResetCharacter)
            {
                foreach (var mission in currentWeeklyLore.WeeklyMissions)
                {
                    if (characterProgress.ContainsKey(mission.Id))
                    {
                        characterProgress.Remove(mission.Id);
                    }
                }
                character.MissionProgressJson = JsonSerializer.Serialize(characterProgress);
            }

            if (shouldResetUser)
            {
                foreach (var mission in currentWeeklyLore.WeeklyMissions)
                {
                    if (userProgress.ContainsKey(mission.Id))
                    {
                        userProgress.Remove(mission.Id);
                    }
                }
                character.User!.MissionProgressJson = JsonSerializer.Serialize(userProgress);
            }
        }

        private int GetWeekNumber(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }
    }
}
