using System.Collections.Generic;

namespace backend.Utils
{
    public class EnemyTemplate
    {
        public string Name { get; set; } = "";
        public int Level { get; set; } = 1;
        public int MaxHp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int XP { get; set; }
        public double CritChance { get; set; }
        public string Type { get; set; } = "normal";
        public string Description { get; set; } = "";
    }

    public static class EnemyTemplates
    {
        public static List<EnemyTemplate> All = new List<EnemyTemplate>
        {
            // Undead
            new EnemyTemplate { Name = "Skeleton", Level = 1, MaxHp = 18, Attack = 11, Defense = 2, XP = 20, CritChance = 0.06, Type = "undead", Description = "Clattering bones held together by dark magic." },
            new EnemyTemplate { Name = "Zombie", Level = 2, MaxHp = 28, Attack = 14, Defense = 3, XP = 34, CritChance = 0.06, Type = "undead", Description = "Slow but relentless. Smells terrible." },
            new EnemyTemplate { Name = "Vampire", Level = 5, MaxHp = 55, Attack = 26, Defense = 8, XP = 90, CritChance = 0.13, Type = "undead", Description = "Elegant and deadly. Thirsts for your blood." },
            new EnemyTemplate { Name = "Lich King", Level = 11, MaxHp = 160, Attack = 46, Defense = 16, XP = 350, CritChance = 0.15, Type = "undead", Description = "Master of forbidden magic. Rules the undead." },

            // Beast
            new EnemyTemplate { Name = "Wolf", Level = 1, MaxHp = 15, Attack = 12, Defense = 2, XP = 16, CritChance = 0.08, Type = "beast", Description = "Quick and cunning hunter of the night." },
            new EnemyTemplate { Name = "Dire Bear", Level = 8, MaxHp = 110, Attack = 32, Defense = 10, XP = 170, CritChance = 0.15, Type = "beast", Description = "Massive and unstoppable in a rampage." },
            new EnemyTemplate { Name = "Giant Spider", Level = 4, MaxHp = 38, Attack = 19, Defense = 5, XP = 55, CritChance = 0.13, Type = "beast", Description = "Venomous and loves dark corners." },

            // Human
            new EnemyTemplate { Name = "Bandit", Level = 2, MaxHp = 24, Attack = 14, Defense = 3, XP = 23, CritChance = 0.07, Type = "human", Description = "Out for your gold, and maybe your life." },
            new EnemyTemplate { Name = "Cultist", Level = 4, MaxHp = 40, Attack = 19, Defense = 5, XP = 60, CritChance = 0.12, Type = "human", Description = "Worships dark gods. Creepy vibes." },
            new EnemyTemplate { Name = "Dark Knight", Level = 10, MaxHp = 140, Attack = 39, Defense = 14, XP = 270, CritChance = 0.14, Type = "human", Description = "Sworn to evil. Armor darker than his soul." },

            // Demon/Devil
            new EnemyTemplate { Name = "Imp", Level = 3, MaxHp = 22, Attack = 18, Defense = 2, XP = 27, CritChance = 0.18, Type = "demon", Description = "Annoying, mischievous, always grinning." },
            new EnemyTemplate { Name = "Demon Brute", Level = 12, MaxHp = 190, Attack = 52, Defense = 18, XP = 400, CritChance = 0.19, Type = "demon", Description = "Huge horns, huge muscles, zero patience." },

            // Dragon
            new EnemyTemplate { Name = "Young Dragon", Level = 15, MaxHp = 270, Attack = 74, Defense = 22, XP = 650, CritChance = 0.20, Type = "dragon", Description = "Fiery breath and an attitude problem." },
            new EnemyTemplate { Name = "Ancient Dragon", Level = 25, MaxHp = 690, Attack = 136, Defense = 36, XP = 1500, CritChance = 0.25, Type = "dragon", Description = "Wiser and deadlier than you can imagine." },

            // Construct 
            new EnemyTemplate { Name = "Stone Golem", Level = 9, MaxHp = 120, Attack = 28, Defense = 20, XP = 190, CritChance = 0.07, Type = "construct", Description = "Solid rock. Not much for conversation." },
            new EnemyTemplate { Name = "Animated Armor", Level = 5, MaxHp = 52, Attack = 22, Defense = 13, XP = 80, CritChance = 0.08, Type = "construct", Description = "Empty armor, full of rage." },

            // Elemental
            new EnemyTemplate { Name = "Fire Elemental", Level = 6, MaxHp = 58, Attack = 27, Defense = 6, XP = 110, CritChance = 0.15, Type = "elemental", Description = "Made of living flame. Toasty!" },
            new EnemyTemplate { Name = "Ice Elemental", Level = 7, MaxHp = 62, Attack = 24, Defense = 10, XP = 115, CritChance = 0.13, Type = "elemental", Description = "Cold, calculating, a bit frosty." },

            // Extras
            new EnemyTemplate { Name = "Haunted Toaster", Level = 1, MaxHp = 10, Attack = 8, Defense = 2, XP = 11, CritChance = 0.22, Type = "construct", Description = "Makes burnt toast... and your life miserable." },
            new EnemyTemplate { Name = "Buff Squirrel", Level = 2, MaxHp = 22, Attack = 19, Defense = 3, XP = 21, CritChance = 0.16, Type = "beast", Description = "Small body, huge biceps." },
            new EnemyTemplate { Name = "Social Media Troll", Level = 3, MaxHp = 29, Attack = 17, Defense = 3, XP = 33, CritChance = 0.15, Type = "human", Description = "Feeds on your rage and typos." }
        };

        public static EnemyTemplate? GetByName(string name)
        {
            return All.Find(e => e.Name == name);
        }
    }
}
