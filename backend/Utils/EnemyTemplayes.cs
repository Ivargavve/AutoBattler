using System.Collections.Generic;

namespace backend.Utils
{
    public class EnemyTemplate
    {
        public string Name { get; set; } = "";
        public int MaxHp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int XP { get; set; }
        public double CritChance { get; set; }
        public string Type { get; set; } = "normal"; 
    }

    public static class EnemyTemplates
    {
        public static List<EnemyTemplate> All = new List<EnemyTemplate>
        {
            // Undead
            new EnemyTemplate { Name = "Skeleton", MaxHp = 15, Attack = 9, Defense = 2, XP = 18, CritChance = 0.06, Type = "undead" },
            new EnemyTemplate { Name = "Zombie", MaxHp = 22, Attack = 10, Defense = 1, XP = 25, CritChance = 0.04, Type = "undead" },
            new EnemyTemplate { Name = "Vampire", MaxHp = 27, Attack = 16, Defense = 3, XP = 58, CritChance = 0.15, Type = "undead" },
            new EnemyTemplate { Name = "Lich King", MaxHp = 45, Attack = 28, Defense = 6, XP = 120, CritChance = 0.12, Type = "undead" },

            // Beast
            new EnemyTemplate { Name = "Wolf", MaxHp = 13, Attack = 11, Defense = 1, XP = 16, CritChance = 0.10, Type = "beast" },
            new EnemyTemplate { Name = "Dire Bear", MaxHp = 30, Attack = 21, Defense = 4, XP = 68, CritChance = 0.13, Type = "beast" },
            new EnemyTemplate { Name = "Giant Spider", MaxHp = 18, Attack = 13, Defense = 2, XP = 30, CritChance = 0.12, Type = "beast" },

            // Human
            new EnemyTemplate { Name = "Bandit", MaxHp = 16, Attack = 12, Defense = 2, XP = 17, CritChance = 0.09, Type = "human" },
            new EnemyTemplate { Name = "Cultist", MaxHp = 20, Attack = 14, Defense = 3, XP = 31, CritChance = 0.13, Type = "human" },
            new EnemyTemplate { Name = "Dark Knight", MaxHp = 35, Attack = 22, Defense = 6, XP = 75, CritChance = 0.12, Type = "human" },

            // Demon/Devil
            new EnemyTemplate { Name = "Imp", MaxHp = 11, Attack = 13, Defense = 0, XP = 13, CritChance = 0.15, Type = "demon" },
            new EnemyTemplate { Name = "Demon Brute", MaxHp = 41, Attack = 26, Defense = 5, XP = 110, CritChance = 0.16, Type = "demon" },

            // Dragon
            new EnemyTemplate { Name = "Young Dragon", MaxHp = 50, Attack = 28, Defense = 7, XP = 140, CritChance = 0.17, Type = "dragon" },
            new EnemyTemplate { Name = "Ancient Dragon", MaxHp = 120, Attack = 42, Defense = 12, XP = 480, CritChance = 0.21, Type = "dragon" },

            // Construct 
            new EnemyTemplate { Name = "Stone Golem", MaxHp = 40, Attack = 17, Defense = 9, XP = 80, CritChance = 0.04, Type = "construct" },
            new EnemyTemplate { Name = "Animated Armor", MaxHp = 23, Attack = 14, Defense = 7, XP = 38, CritChance = 0.07, Type = "construct" },

            // Elemental
            new EnemyTemplate { Name = "Fire Elemental", MaxHp = 25, Attack = 19, Defense = 2, XP = 55, CritChance = 0.13, Type = "elemental" },
            new EnemyTemplate { Name = "Ice Elemental", MaxHp = 23, Attack = 16, Defense = 3, XP = 51, CritChance = 0.11, Type = "elemental" },

            // Extras
            new EnemyTemplate { Name = "Haunted Toaster", MaxHp = 8, Attack = 6, Defense = 1, XP = 9, CritChance = 0.18, Type = "construct" },
            new EnemyTemplate { Name = "Buff Squirrel", MaxHp = 12, Attack = 14, Defense = 1, XP = 13, CritChance = 0.15, Type = "beast" },
            new EnemyTemplate { Name = "Social Media Troll", MaxHp = 16, Attack = 13, Defense = 1, XP = 19, CritChance = 0.15, Type = "human" }
        };

        public static EnemyTemplate? GetByName(string name)
        {
            return All.Find(e => e.Name == name);
        }
    }
}
