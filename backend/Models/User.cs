  namespace backend.Models
{  
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "User";

        public string FullName { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string GoogleId { get; set; } = string.Empty;

        public int ExperiencePoints { get; set; } = 0;
        public int Level { get; set; } = 1;
        public int Credits { get; set; } = 0;

        public string CosmeticItemsJson { get; set; } = string.Empty;
        public string SettingsJson { get; set; } = string.Empty;
        public string AchievementsJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        public ICollection<Friendship> FriendshipsInitiated { get; set; } = new List<Friendship>();
        public ICollection<Friendship> FriendshipsReceived { get; set; } = new List<Friendship>();
    }
}