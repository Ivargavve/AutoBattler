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
        public int CreditsMin { get; set; }
        public int CreditsMax { get; set; }
        public string ImageUrl { get; set; } = "";
    }

    public static class EnemyTemplates
    {
        public static List<EnemyTemplate> All = new List<EnemyTemplate>
        {
            // Undead
            new EnemyTemplate { Name = "Skeleton", Level = 1, MaxHp = 18, Attack = 11, Defense = 2, XP = 120, CritChance = 0.06, Type = "undead", Description = "Clattering bones held together by dark magic.", CreditsMin = 1, CreditsMax = 10, ImageUrl = "assets/monsters/skeleton.jpg" },
            new EnemyTemplate { Name = "Zombie", Level = 2, MaxHp = 28, Attack = 14, Defense = 3, XP = 134, CritChance = 0.06, Type = "undead", Description = "Slow but relentless. Smells terrible.", CreditsMin = 5, CreditsMax = 15, ImageUrl = "assets/monsters/zombie.jpg" },
            new EnemyTemplate { Name = "Vampire", Level = 5, MaxHp = 55, Attack = 26, Defense = 8, XP = 190, CritChance = 0.13, Type = "undead", Description = "Elegant and deadly. Thirsts for your blood.", CreditsMin = 50, CreditsMax = 120, ImageUrl = "assets/monsters/vampire.jpg" },
            new EnemyTemplate { Name = "Lich King", Level = 11, MaxHp = 160, Attack = 46, Defense = 16, XP = 1350, CritChance = 0.15, Type = "undead", Description = "Master of forbidden magic. Rules the undead.", CreditsMin = 300, CreditsMax = 700, ImageUrl = "assets/monsters/lichking.jpg" },

            // Beast
            new EnemyTemplate { Name = "Wolf", Level = 1, MaxHp = 15, Attack = 12, Defense = 2, XP = 116, CritChance = 0.08, Type = "beast", Description = "Quick and cunning hunter of the night.", CreditsMin = 2, CreditsMax = 8, ImageUrl = "assets/monsters/wolf.jpg" },
            new EnemyTemplate { Name = "Dire Bear", Level = 8, MaxHp = 110, Attack = 32, Defense = 10, XP = 1170, CritChance = 0.15, Type = "beast", Description = "Massive and unstoppable in a rampage.", CreditsMin = 120, CreditsMax = 250, ImageUrl = "assets/monsters/direbear.jpg" },
            new EnemyTemplate { Name = "Giant Spider", Level = 4, MaxHp = 38, Attack = 19, Defense = 5, XP = 155, CritChance = 0.13, Type = "beast", Description = "Venomous and loves dark corners.", CreditsMin = 20, CreditsMax = 40, ImageUrl = "assets/monsters/giantspider.jpg" },

            // Human
            new EnemyTemplate { Name = "Bandit", Level = 2, MaxHp = 24, Attack = 14, Defense = 3, XP = 123, CritChance = 0.07, Type = "human", Description = "Out for your gold, and maybe your life.", CreditsMin = 10, CreditsMax = 25, ImageUrl = "assets/monsters/bandit.jpg" },
            new EnemyTemplate { Name = "Cultist", Level = 4, MaxHp = 40, Attack = 19, Defense = 5, XP = 160, CritChance = 0.12, Type = "human", Description = "Worships dark gods. Creepy vibes.", CreditsMin = 20, CreditsMax = 45, ImageUrl = "assets/monsters/cultist.jpg" },
            new EnemyTemplate { Name = "Dark Knight", Level = 10, MaxHp = 140, Attack = 39, Defense = 114, XP = 270, CritChance = 0.14, Type = "human", Description = "Sworn to evil. Armor darker than his soul.", CreditsMin = 180, CreditsMax = 350, ImageUrl = "assets/monsters/darkknight.jpg" },

            // Demon/Devil
            new EnemyTemplate { Name = "Imp", Level = 3, MaxHp = 22, Attack = 18, Defense = 2, XP = 127, CritChance = 0.18, Type = "demon", Description = "Annoying, mischievous, always grinning.", CreditsMin = 15, CreditsMax = 30, ImageUrl = "assets/monsters/imp.jpg" },
            new EnemyTemplate { Name = "Demon Brute", Level = 12, MaxHp = 190, Attack = 52, Defense = 118, XP = 400, CritChance = 0.19, Type = "demon", Description = "Huge horns, huge muscles, zero patience.", CreditsMin = 300, CreditsMax = 600, ImageUrl = "assets/monsters/demonbrute.jpg" },

            // Dragon
            new EnemyTemplate { Name = "Young Dragon", Level = 15, MaxHp = 270, Attack = 74, Defense = 22, XP = 1650, CritChance = 0.20, Type = "dragon", Description = "Fiery breath and an attitude problem.", CreditsMin = 300, CreditsMax = 600, ImageUrl = "assets/monsters/youngdragon.jpg" },
            new EnemyTemplate { Name = "Ancient Dragon", Level = 25, MaxHp = 690, Attack = 136, Defense = 36, XP = 11500, CritChance = 0.25, Type = "dragon", Description = "Wiser and deadlier than you can imagine.", CreditsMin = 1000, CreditsMax = 2000, ImageUrl = "assets/monsters/ancientdragon.jpg" },

            // Construct 
            new EnemyTemplate { Name = "Stone Golem", Level = 9, MaxHp = 120, Attack = 28, Defense = 20, XP = 1190, CritChance = 0.07, Type = "construct", Description = "Solid rock. Not much for conversation.", CreditsMin = 140, CreditsMax = 280, ImageUrl = "assets/monsters/stonegolem.jpg" },
            new EnemyTemplate { Name = "Animated Armor", Level = 5, MaxHp = 52, Attack = 22, Defense = 13, XP = 180, CritChance = 0.08, Type = "construct", Description = "Empty armor, full of rage.", CreditsMin = 40, CreditsMax = 80, ImageUrl = "assets/monsters/animatedarmor.jpg" },

            // Elemental
            new EnemyTemplate { Name = "Fire Elemental", Level = 6, MaxHp = 58, Attack = 27, Defense = 6, XP = 1110, CritChance = 0.15, Type = "elemental", Description = "Made of living flame. Toasty!", CreditsMin = 90, CreditsMax = 170, ImageUrl = "assets/monsters/fireelemental.jpg" },
            new EnemyTemplate { Name = "Ice Elemental", Level = 7, MaxHp = 62, Attack = 24, Defense = 10, XP = 1115, CritChance = 0.13, Type = "elemental", Description = "Cold, calculating, a bit frosty.", CreditsMin = 100, CreditsMax = 180, ImageUrl = "assets/monsters/icelemental.jpg" },

            // Extras
            new EnemyTemplate { Name = "Haunted Toaster", Level = 1, MaxHp = 10, Attack = 8, Defense = 2, XP = 111, CritChance = 0.22, Type = "construct", Description = "Makes burnt toast... and your life miserable.", CreditsMin = 1, CreditsMax = 6, ImageUrl = "assets/monsters/hauntedtoaster.jpg" },
            new EnemyTemplate { Name = "Buff Squirrel", Level = 2, MaxHp = 22, Attack = 19, Defense = 3, XP = 121, CritChance = 0.16, Type = "beast", Description = "Small body, huge biceps.", CreditsMin = 8, CreditsMax = 16, ImageUrl = "assets/monsters/buffasquirrel.jpg" },
            new EnemyTemplate { Name = "Social Media Troll", Level = 3, MaxHp = 29, Attack = 17, Defense = 3, XP = 133, CritChance = 0.15, Type = "human", Description = "Feeds on your rage and typos.", CreditsMin = 12, CreditsMax = 24, ImageUrl = "assets/monsters/socialmediatroll.jpg" }
        };

        public static EnemyTemplate? GetByName(string name)
        {
            return All.Find(e => e.Name == name);
        }
    }
}
