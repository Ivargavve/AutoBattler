// backend/Data/AttackTemplates.cs

namespace backend.Data
{
    public class AttackTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string DamageType { get; set; } = "";
        public int BaseDamage { get; set; }
        public int MaxCharges { get; set; }
        public Dictionary<string, double> Scaling { get; set; } = new();
        public Dictionary<string, int> RequiredStats { get; set; } = new();
        public List<string> AllowedClasses { get; set; } = new();
        public string Description { get; set; } = "";
        public int HealAmount { get; set; } = 0;
        public bool BlockNextAttack { get; set; } = false;
        public bool Poison { get; set; } = false;
        public bool EvadeNextAttack { get; set; } = false;
        public int CritChanceBonus { get; set; } = 0;
        public int CritBonusTurns { get; set; } = 0;
        public int PoisonDamagePerTurn { get; set; } = 0;
        public int PoisonDuration { get; set; } = 0;

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
                DamageType = "slashing",
                BaseDamage = 10,
                MaxCharges = 8,
                Scaling = new Dictionary<string, double> { { "attack", 1.1 }, { "agility", 0.2 } },
                RequiredStats = new Dictionary<string, int> { { "attack", 5 } },
                AllowedClasses = new List<string> { "warrior" },
                Description = "A quick slashing attack."
            },
            new AttackTemplate
            {
                Id = 2,
                Name = "Cleave",
                Type = "physical",
                DamageType = "slashing",
                BaseDamage = 14,
                MaxCharges = 5,
                Scaling = new Dictionary<string, double> { { "attack", 1.3 }, { "speed", 0.2 } },
                RequiredStats = new Dictionary<string, int> { { "attack", 12 } },
                AllowedClasses = new List<string> { "warrior" },
                Description = "A heavy slashing attack with high base damage."
            },
            new AttackTemplate
            {
                Id = 3,
                Name = "Shield Block",
                Type = "defense",
                DamageType = "none",
                BaseDamage = 2, 
                MaxCharges = 3,
                Scaling = new Dictionary<string, double> { { "defense", 1.0 } },
                RequiredStats = new Dictionary<string, int> { { "defense", 10 } },
                AllowedClasses = new List<string> { "warrior" },
                BlockNextAttack = true,
                Description = "Blocks the next incoming attack and deals minimal damage."
            },
            new AttackTemplate
            {
                Id = 4,
                Name = "Battle Shout",
                Type = "buff",
                DamageType = "none",
                BaseDamage = 0,
                MaxCharges = 2,
                Scaling = new Dictionary<string, double> { },
                RequiredStats = new Dictionary<string, int> { { "attack", 8 } },
                AllowedClasses = new List<string> { "warrior" },
                CritChanceBonus = 100,
                CritBonusTurns = 2,
                Description = "Increases your attack and critical chance for a short time."
            },
            new AttackTemplate
            {
                Id = 5,
                Name = "Smite",
                Type = "magic",
                DamageType = "holy",
                BaseDamage = 12,
                MaxCharges = 6,
                Scaling = new Dictionary<string, double> { { "magic", 1.0 }, { "attack", 0.5 } },
                RequiredStats = new Dictionary<string, int> { { "magic", 9 } },
                AllowedClasses = new List<string> { "paladin" },
                Description = "A holy strike against evil."
            },
            new AttackTemplate
            {
                Id = 6,
                Name = "Holy Light",
                Type = "magic",
                DamageType = "holy",
                BaseDamage = 0,
                MaxCharges = 3,
                Scaling = new Dictionary<string, double> { { "magic", 1.5 } },
                RequiredStats = new Dictionary<string, int> { { "magic", 12 } },
                AllowedClasses = new List<string> { "paladin" },
                HealAmount = 22,
                Description = "Heals the user for a large amount. Does extra damage to undead."
            },
            new AttackTemplate
            {
                Id = 7,
                Name = "Sacred Shield",
                Type = "defense",
                DamageType = "none",
                BaseDamage = 2, // Liten skada
                MaxCharges = 2,
                Scaling = new Dictionary<string, double> { { "defense", 1.2 } },
                RequiredStats = new Dictionary<string, int> { { "defense", 11 } },
                AllowedClasses = new List<string> { "paladin" },
                BlockNextAttack = true,
                Description = "Blocks the next incoming attack and grants a small heal."
            },
            new AttackTemplate
            {
                Id = 8,
                Name = "Judgement",
                Type = "magic",
                DamageType = "holy",
                BaseDamage = 16,
                MaxCharges = 4,
                Scaling = new Dictionary<string, double> { { "magic", 1.4 } },
                RequiredStats = new Dictionary<string, int> { { "magic", 14 } },
                AllowedClasses = new List<string> { "paladin" },
                Description = "Delivers divine judgement on the enemy."
            },

            new AttackTemplate
            {
                Id = 9,
                Name = "Fireball",
                Type = "magic",
                DamageType = "fire",
                BaseDamage = 13,
                MaxCharges = 6,
                Scaling = new Dictionary<string, double> { { "magic", 1.1 }, { "attack", 0.2 } },
                RequiredStats = new Dictionary<string, int> { { "magic", 10 } },
                AllowedClasses = new List<string> { "mage" },
                Description = "A burning magical fireball."
            },
            new AttackTemplate
            {
                Id = 10,
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
                Id = 11,
                Name = "Arcane Blast",
                Type = "magic",
                DamageType = "arcane",
                BaseDamage = 15,
                MaxCharges = 4,
                Scaling = new Dictionary<string, double> { { "magic", 1.4 } },
                RequiredStats = new Dictionary<string, int> { { "magic", 15 } },
                AllowedClasses = new List<string> { "mage" },
                Description = "A powerful arcane blast."
            },
            new AttackTemplate
            {
                Id = 12,
                Name = "Mana Shield",
                Type = "defense",
                DamageType = "none",
                BaseDamage = 0,
                MaxCharges = 3,
                Scaling = new Dictionary<string, double> { { "magic", 0.7 } },
                RequiredStats = new Dictionary<string, int> { { "magic", 10 } },
                AllowedClasses = new List<string> { "mage" },
                BlockNextAttack = true,
                Description = "Creates a shield of mana to absorb the next attack."
            },

            new AttackTemplate
            {
                Id = 13,
                Name = "Quick Shot",
                Type = "physical",
                DamageType = "piercing",
                BaseDamage = 9,
                MaxCharges = 8,
                Scaling = new Dictionary<string, double> { { "attack", 0.7 }, { "agility", 0.8 } },
                RequiredStats = new Dictionary<string, int> { { "agility", 9 } },
                AllowedClasses = new List<string> { "rogue" },
                Description = "A swift ranged attack."
            },
            new AttackTemplate
            {
                Id = 14,
                Name = "Poison Strike",
                Type = "physical",
                DamageType = "poison",
                BaseDamage = 7,
                MaxCharges = 9,
                Scaling = new Dictionary<string, double> { { "attack", 0.8 }, { "agility", 0.5 } },
                RequiredStats = new Dictionary<string, int> { { "agility", 10 } },
                AllowedClasses = new List<string> { "rogue" },
                Poison = true,
                PoisonDamagePerTurn = 3,
                PoisonDuration = 3,
                Description = "A strike that poisons the enemy over time."
            },

            new AttackTemplate
            {
                Id = 15,
                Name = "Shadowstep",
                Type = "utility",
                DamageType = "none",
                BaseDamage = 0,
                MaxCharges = 3,
                Scaling = new Dictionary<string, double> { { "agility", 1.0 } },
                RequiredStats = new Dictionary<string, int> { { "agility", 12 } },
                AllowedClasses = new List<string> { "rogue" },
                EvadeNextAttack = true,
                CritChanceBonus = 85,
                CritBonusTurns = 1,
                Description = "Evades the next attack and increases your critical chance for 1 turn."
            },
            new AttackTemplate
            {
                Id = 16,
                Name = "Backstab",
                Type = "physical",
                DamageType = "piercing",
                BaseDamage = 15,
                MaxCharges = 3,
                Scaling = new Dictionary<string, double> { { "attack", 1.3 } },
                RequiredStats = new Dictionary<string, int> { { "agility", 11 } },
                AllowedClasses = new List<string> { "rogue" },
                Description = "Deals massive damage when striking from the shadows."
            },

            new AttackTemplate
            {
                Id = 17,
                Name = "Piercing Arrow",
                Type = "physical",
                DamageType = "piercing",
                BaseDamage = 11,
                MaxCharges = 7,
                Scaling = new Dictionary<string, double> { { "attack", 0.9 }, { "agility", 0.7 } },
                RequiredStats = new Dictionary<string, int> { { "agility", 10 } },
                AllowedClasses = new List<string> { "ranger" },
                Description = "An arrow shot that pierces armor."
            },
            new AttackTemplate
            {
                Id = 18,
                Name = "Multi-Shot",
                Type = "physical",
                DamageType = "piercing",
                BaseDamage = 7,
                MaxCharges = 5,
                Scaling = new Dictionary<string, double> { { "attack", 0.5 }, { "agility", 0.5 } },
                RequiredStats = new Dictionary<string, int> { { "agility", 12 } },
                AllowedClasses = new List<string> { "ranger" },
                Description = "Fires multiple arrows at once for combined damage."
            },
            new AttackTemplate
            {
                Id = 19,
                Name = "Nature's Grasp",
                Type = "magic",
                DamageType = "poison",
                BaseDamage = 8,
                MaxCharges = 4,
                Scaling = new Dictionary<string, double> { { "magic", 0.9 } },
                RequiredStats = new Dictionary<string, int> { { "magic", 8 } },
                AllowedClasses = new List<string> { "ranger" },
                Poison = true,
                PoisonDamagePerTurn = 4,
                PoisonDuration = 2,
                Description = "Roots the enemy and deals poison damage over time."
            },
            new AttackTemplate
            {
                Id = 20,
                Name = "Camouflage",
                Type = "utility",
                DamageType = "none",
                BaseDamage = 0,
                MaxCharges = 2,
                Scaling = new Dictionary<string, double> { { "agility", 1.2 } },
                RequiredStats = new Dictionary<string, int> { { "agility", 13 } },
                AllowedClasses = new List<string> { "ranger" },
                EvadeNextAttack = true,
                CritChanceBonus = 55,
                CritBonusTurns = 1,
                Description = "Evades the next attack and increases your chance to land a critical hit."
            }
        };
    }
}
