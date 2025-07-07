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
    }

    public static class EnemyTemplates
    {
        public static List<EnemyTemplate> All = new List<EnemyTemplate>
        {
            new EnemyTemplate { Name = "Goblin", MaxHp = 20, Attack = 20, Defense = 1, XP = 40, CritChance = 0.05 },
            new EnemyTemplate { Name = "Rat King", MaxHp = 24, Attack = 32, Defense = 2, XP = 60, CritChance = 0.08 },
            new EnemyTemplate { Name = "Angry Chicken", MaxHp = 16, Attack = 17, Defense = 0, XP = 25, CritChance = 0.12 },
            new EnemyTemplate { Name = "Skeleton Mage", MaxHp = 28, Attack = 35, Defense = 4, XP = 90, CritChance = 0.10 },
            new EnemyTemplate { Name = "Karen", MaxHp = 30, Attack = 14, Defense = 3, XP = 120, CritChance = 0.06 },
            new EnemyTemplate { Name = "Pixel Slime", MaxHp = 18, Attack = 18, Defense = 1, XP = 22, CritChance = 0.03 },
            new EnemyTemplate { Name = "Mime", MaxHp = 21, Attack = 9, Defense = 2, XP = 45, CritChance = 0.15 },
            new EnemyTemplate { Name = "AI Dungeon Master", MaxHp = 50, Attack = 27, Defense = 6, XP = 200, CritChance = 0.09 },
            new EnemyTemplate { Name = "Trash Panda", MaxHp = 18, Attack = 14, Defense = 2, XP = 33, CritChance = 0.07 },
            new EnemyTemplate { Name = "Swole Hamster", MaxHp = 15, Attack = 20, Defense = 0, XP = 29, CritChance = 0.10 },
            new EnemyTemplate { Name = "Swag Skeleton", MaxHp = 26, Attack = 21, Defense = 4, XP = 65, CritChance = 0.08 },
            new EnemyTemplate { Name = "Dad Joke Golem", MaxHp = 34, Attack = 11, Defense = 8, XP = 55, CritChance = 0.06 },
            new EnemyTemplate { Name = "Shadow Influencer", MaxHp = 40, Attack = 30, Defense = 3, XP = 110, CritChance = 0.13 },
            new EnemyTemplate { Name = "Flat Earth Lizard", MaxHp = 25, Attack = 17, Defense = 2, XP = 47, CritChance = 0.12 },
            new EnemyTemplate { Name = "Dragon Daddy", MaxHp = 300, Attack = 34, Defense = 9, XP = 300, CritChance = 0.22 },
            new EnemyTemplate { Name = "Angry Barista", MaxHp = 18, Attack = 19, Defense = 1, XP = 39, CritChance = 0.11 },
            new EnemyTemplate { Name = "Social Media Troll", MaxHp = 19, Attack = 15, Defense = 0, XP = 38, CritChance = 0.17 },
            new EnemyTemplate { Name = "Crypto Bro", MaxHp = 21, Attack = 24, Defense = 1, XP = 41, CritChance = 0.09 },
            new EnemyTemplate { Name = "Cursed Roomba", MaxHp = 13, Attack = 10, Defense = 0, XP = 21, CritChance = 0.15 },
            new EnemyTemplate { Name = "Ninja Grandma", MaxHp = 20, Attack = 31, Defense = 3, XP = 90, CritChance = 0.16 },
            new EnemyTemplate { Name = "Duck with Sunglasses", MaxHp = 16, Attack = 12, Defense = 2, XP = 27, CritChance = 0.14 },
            new EnemyTemplate { Name = "Glitched Cat", MaxHp = 15, Attack = 16, Defense = 1, XP = 35, CritChance = 0.17 },
            new EnemyTemplate { Name = "Loch Ness Bro", MaxHp = 32, Attack = 28, Defense = 5, XP = 150, CritChance = 0.10 },
            new EnemyTemplate { Name = "Baby Yeti", MaxHp = 22, Attack = 14, Defense = 2, XP = 36, CritChance = 0.12 },
            new EnemyTemplate { Name = "Caffeinated Ferret", MaxHp = 14, Attack = 18, Defense = 1, XP = 23, CritChance = 0.15 },
            new EnemyTemplate { Name = "Space Otter", MaxHp = 23, Attack = 21, Defense = 2, XP = 45, CritChance = 0.11 },
            new EnemyTemplate { Name = "Haunted Toaster", MaxHp = 10, Attack = 7, Defense = 0, XP = 12, CritChance = 0.19 },
            new EnemyTemplate { Name = "Boomer Mage", MaxHp = 29, Attack = 25, Defense = 4, XP = 88, CritChance = 0.11 },
            new EnemyTemplate { Name = "Unpaid Intern", MaxHp = 12, Attack = 9, Defense = 1, XP = 14, CritChance = 0.02 },
            new EnemyTemplate { Name = "Flatulent Ogre", MaxHp = 36, Attack = 23, Defense = 3, XP = 78, CritChance = 0.10 },
            new EnemyTemplate { Name = "Buff Pigeon", MaxHp = 20, Attack = 19, Defense = 2, XP = 30, CritChance = 0.16 },
            new EnemyTemplate { Name = "Radioactive Squirrel", MaxHp = 16, Attack = 15, Defense = 2, XP = 28, CritChance = 0.13 },
            new EnemyTemplate { Name = "Possessed Fridge", MaxHp = 27, Attack = 23, Defense = 4, XP = 62, CritChance = 0.09 },
            new EnemyTemplate { Name = "Karen Jr.", MaxHp = 16, Attack = 8, Defense = 1, XP = 19, CritChance = 0.15 },
            new EnemyTemplate { Name = "Cosplay Vampire", MaxHp = 18, Attack = 17, Defense = 2, XP = 31, CritChance = 0.14 },
            new EnemyTemplate { Name = "Buff Shiba Inu", MaxHp = 21, Attack = 24, Defense = 2, XP = 46, CritChance = 0.17 },
            new EnemyTemplate { Name = "Sad Clown", MaxHp = 19, Attack = 10, Defense = 2, XP = 23, CritChance = 0.18 },
            new EnemyTemplate { Name = "Tinfoil Knight", MaxHp = 25, Attack = 16, Defense = 4, XP = 48, CritChance = 0.13 },
            new EnemyTemplate { Name = "Kung Fu Grandma", MaxHp = 22, Attack = 27, Defense = 3, XP = 54, CritChance = 0.14 },
            new EnemyTemplate { Name = "Buff Seagull", MaxHp = 17, Attack = 22, Defense = 2, XP = 35, CritChance = 0.15 },
            new EnemyTemplate { Name = "Dad in Sandals", MaxHp = 26, Attack = 13, Defense = 5, XP = 40, CritChance = 0.08 },
            new EnemyTemplate { Name = "Haunted Gaming Chair", MaxHp = 20, Attack = 18, Defense = 3, XP = 39, CritChance = 0.12 },
            new EnemyTemplate { Name = "Woke Witch", MaxHp = 24, Attack = 29, Defense = 4, XP = 70, CritChance = 0.13 },
            new EnemyTemplate { Name = "Buff Duck", MaxHp = 17, Attack = 19, Defense = 2, XP = 34, CritChance = 0.16 },
            new EnemyTemplate { Name = "Sleep-Deprived Student", MaxHp = 14, Attack = 15, Defense = 1, XP = 25, CritChance = 0.09 }
        };

        public static EnemyTemplate? GetByName(string name)
        {
            return All.Find(e => e.Name == name);
        }
    }
}
