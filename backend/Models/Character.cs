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

        public int MaxHealth { get; set; } = 100;
        public int CurrentHealth { get; set; } = 100;
        public int MaxEnergy { get; set; } = 3;
        public int CurrentEnergy { get; set; } = 3;

        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 5;
        public int Agility { get; set; } = 5;
        public double CriticalChance { get; set; } = 0.05; 

        public int Credits { get; set; } = 0;
        public string InventoryJson { get; set; } = string.Empty;
        public string EquipmentJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
