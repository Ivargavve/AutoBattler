using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class WeeklyLore
    {
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
        [JsonPropertyName("weeklyMissions")]
        public List<WeeklyMission> WeeklyMissions { get; set; } = new List<WeeklyMission>();
    }

    public class WeeklyMission
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("rewardType")]
        public string RewardType { get; set; } = string.Empty; // "character" or "user"
        [JsonPropertyName("rewardAmount")]
        public int RewardAmount { get; set; }
        [JsonPropertyName("rewardItem")]
        public string RewardItem { get; set; } = string.Empty; // "xp" or "credits"
    }

    public class DailyMission
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("rewardType")]
        public string RewardType { get; set; } = string.Empty; // "character" or "user"
        [JsonPropertyName("rewardAmount")]
        public int RewardAmount { get; set; }
        [JsonPropertyName("rewardItem")]
        public string RewardItem { get; set; } = string.Empty; // "xp" or "credits"
    }

    public class TalesResponse
    {
        [JsonPropertyName("currentLore")]
        public WeeklyLore CurrentLore { get; set; } = new WeeklyLore();
        [JsonPropertyName("dailyMissions")]
        public List<DailyMission> DailyMissions { get; set; } = new List<DailyMission>();
        [JsonPropertyName("weeklyMissions")]
        public List<WeeklyMission> WeeklyMissions { get; set; } = new List<WeeklyMission>();
        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }
        [JsonPropertyName("nextDailyReset")]
        public DateTime NextDailyReset { get; set; }
        [JsonPropertyName("nextWeeklyReset")]
        public DateTime NextWeeklyReset { get; set; }
        [JsonPropertyName("missionProgress")]
        public Dictionary<string, int> MissionProgress { get; set; } = new Dictionary<string, int>();
        [JsonPropertyName("claimedMissions")]
        public Dictionary<string, DateTime> ClaimedMissions { get; set; } = new Dictionary<string, DateTime>();
    }

    public class MissionClaimRequest
    {
        [Required]
        public string MissionId { get; set; } = string.Empty;
        [Required]
        public string MissionType { get; set; } = string.Empty; // "daily" or "weekly"
    }

    public class MissionClaimResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RewardAmount { get; set; }
        public string RewardType { get; set; } = string.Empty;
        public string RewardItem { get; set; } = string.Empty;
        [JsonPropertyName("character")]
        public object? Character { get; set; } // Include character data when rewardType is "character"
        [JsonPropertyName("user")]
        public object? User { get; set; } // Include user data when rewardType is "user"
    }

    public class MissionProgressUpdateRequest
    {
        [Required]
        public string MissionId { get; set; } = string.Empty;
        [Required]
        public int Progress { get; set; }
        public int? CharacterId { get; set; }
    }
}
