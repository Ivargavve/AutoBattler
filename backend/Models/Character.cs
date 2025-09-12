namespace backend.Models
{
    public class Character
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string ProfileIconUrl { get; set; } = string.Empty;

        public int Level { get; set; } = 1;
        public int ExperiencePoints { get; set; } = 0;
        public int MaxExperiencePoints { get; set; } = 1000;

        public int MaxHealth { get; set; } = 100;
        public int CurrentHealth { get; set; } = 100;
        public int MaxEnergy { get; set; } = 5;
        public int CurrentEnergy { get; set; } = 5;

        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 5;
        public int Agility { get; set; } = 5;
        public int Magic { get; set; } = 0;      
        public int Speed { get; set; } = 0;     
        public int UnspentStatPoints { get; set; } = 0;

        public int HealAmount { get; set; } = 0;
        public int PoisonedTurns { get; set; } = 0; 
        public bool IsBlocking { get; set; } = false; 
        public double CriticalChance { get; set; } = 0.05;

        public int Credits { get; set; } = 0;
        public string InventoryJson { get; set; } = string.Empty;
        public string EquipmentJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastRechargeTime { get; set; } = DateTime.UtcNow;
        public string AttacksJson { get; set; } = "";

        // Mission progression tracking
        public string MissionProgressJson { get; set; } = "{}";
        public string ClaimedMissionsJson { get; set; } = "{}";
        public string LastDailyResetJson { get; set; } = "";
        public string LastWeeklyResetJson { get; set; } = "";
    }
}
