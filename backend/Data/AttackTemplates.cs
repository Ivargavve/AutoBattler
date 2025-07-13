// backend/Data/AttackTemplates.cs

namespace backend.Data
{
    public class AttackTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = ""; // physical, magic, etc.
        public string DamageType { get; set; } = "";
        public int BaseDamage { get; set; }
        public int MaxCharges { get; set; }
        public Dictionary<string, double> Scaling { get; set; } = new();
        public Dictionary<string, int> RequiredStats { get; set; } = new();
        public List<string> AllowedClasses { get; set; } = new();
        public string Description { get; set; } = "";
    }

    public static class AttackTemplates
    {
        public static List<AttackTemplate> All = new List<AttackTemplate>
        {
            new AttackTemplate
            {
                Id = 1,
                Name = "Slash",
                Type = "physical",
                DamageType = "blunt",
                BaseDamage = 10,
                MaxCharges = 8,
                Scaling = new Dictionary<string, double> { { "attack", 1.0 }, { "agility", 0.2 } },
                RequiredStats = new Dictionary<string, int> { { "attack", 5 } },
                AllowedClasses = new List<string> { "warrior" },
                Description = "A quick physical attack."
            },
            new AttackTemplate
            {
                Id = 2,
                Name = "Fireball",
                Type = "magic",
                DamageType = "fire",
                BaseDamage = 12,
                MaxCharges = 5,
                Scaling = new Dictionary<string, double> { { "magic", 1.0 }, { "attack", 0.2 } },
                RequiredStats = new Dictionary<string, int> { { "magic", 10 } },
                AllowedClasses = new List<string> { "mage" },
                Description = "A burning magical fireball."
            },
            new AttackTemplate
            {
                Id = 3,
                Name = "Shield Block",
                Type = "defense",
                DamageType = "none",
                BaseDamage = 0,
                MaxCharges = 3,
                Scaling = new Dictionary<string, double> { { "defense", 1.2 } },
                RequiredStats = new Dictionary<string, int> { { "defense", 10 } },
                AllowedClasses = new List<string> { "warrior", "paladin" },
                Description = "Block the next incoming attack, reducing damage taken."
            },
            new AttackTemplate
            {
                Id = 4,
                Name = "Ice Shard",
                Type = "magic",
                DamageType = "ice",
                BaseDamage = 11,
                MaxCharges = 7,
                Scaling = new Dictionary<string, double> { { "magic", 1.0 }, { "agility", 0.3 } },
                RequiredStats = new Dictionary<string, int> { { "magic", 8 } },
                AllowedClasses = new List<string> { "mage" },
                Description = "A shard of ice that chills the enemy."
            },
            new AttackTemplate
            {
                Id = 5,
                Name = "Cleave",
                Type = "physical",
                DamageType = "slashing",
                BaseDamage = 14,
                MaxCharges = 6,
                Scaling = new Dictionary<string, double> { { "attack", 1.2 }, { "speed", 0.2 } },
                RequiredStats = new Dictionary<string, int> { { "attack", 12 } },
                AllowedClasses = new List<string> { "warrior", "paladin" },
                Description = "A heavy cleaving attack that hits hard."
            },
            new AttackTemplate
            {
                Id = 6,
                Name = "Quick Shot",
                Type = "physical",
                DamageType = "piercing",
                BaseDamage = 8,
                MaxCharges = 6,
                Scaling = new Dictionary<string, double> { { "attack", 0.7 }, { "agility", 0.8 } },
                RequiredStats = new Dictionary<string, int> { { "agility", 9 } },
                AllowedClasses = new List<string> { "ranger", "rogue" },
                Description = "A swift ranged attack."
            },
            new AttackTemplate
            {
                Id = 7,
                Name = "Holy Light",
                Type = "magic",
                DamageType = "holy",
                BaseDamage = 15,
                MaxCharges = 3,
                Scaling = new Dictionary<string, double> { { "magic", 1.3 } },
                RequiredStats = new Dictionary<string, int> { { "magic", 12 } },
                AllowedClasses = new List<string> { "paladin" },
                Description = "A holy spell that damages undead and heals the user."
            },
            new AttackTemplate
            {
                Id = 8,
                Name = "Poison Strike",
                Type = "physical",
                DamageType = "poison",
                BaseDamage = 9,
                MaxCharges = 9,
                Scaling = new Dictionary<string, double> { { "attack", 0.8 }, { "agility", 0.5 } },
                RequiredStats = new Dictionary<string, int> { { "agility", 10 } },
                AllowedClasses = new List<string> { "rogue" },
                Description = "A strike that poisons the enemy over time."
            }
        };
    }
}
