using System.Collections.Generic;

namespace backend.Utils
{
    public class ItemTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = "";
        public string Slot { get; set; } = "";
        public string Rarity { get; set; } = "common";
        public string ImageUrl { get; set; } = "";
        public Dictionary<string, int> StatBonuses { get; set; } = new();
        public int RequiredLevel { get; set; } = 1;
        public string RequiredClass { get; set; } = "";
    }

    public static class ItemTemplates
    {
        public static List<ItemTemplate> All = new List<ItemTemplate>
        {
            // Weapons
            new ItemTemplate
            {
                Id = 1,
                Name = "Iron Sword",
                Description = "A sturdy iron blade that increases attack power.",
                Type = "Weapon",
                Slot = "weapon",
                Rarity = "common",
                ImageUrl = "/assets/items/iron-sword.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 5 } },
                RequiredLevel = 1,
                RequiredClass = "warrior"
            },
            new ItemTemplate
            {
                Id = 2,
                Name = "Steel Greatsword",
                Description = "A massive two-handed sword with devastating power.",
                Type = "Weapon",
                Slot = "weapon",
                Rarity = "uncommon",
                ImageUrl = "/assets/items/steel-greatsword.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 12 }, { "defense", 2 } },
                RequiredLevel = 5,
                RequiredClass = "warrior"
            },
            new ItemTemplate
            {
                Id = 3,
                Name = "Holy Mace",
                Description = "A blessed mace that channels divine power.",
                Type = "Weapon",
                Slot = "weapon",
                Rarity = "rare",
                ImageUrl = "/assets/items/holy-mace.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 8 }, { "magic", 6 } },
                RequiredLevel = 8,
                RequiredClass = "paladin"
            },
            new ItemTemplate
            {
                Id = 4,
                Name = "Mystic Staff",
                Description = "An enchanted staff that amplifies magical abilities.",
                Type = "Weapon",
                Slot = "weapon",
                Rarity = "uncommon",
                ImageUrl = "/assets/items/mystic-staff.png",
                StatBonuses = new Dictionary<string, int> { { "magic", 10 } },
                RequiredLevel = 3,
                RequiredClass = "mage"
            },

            // Armor
            new ItemTemplate
            {
                Id = 5,
                Name = "Leather Armor",
                Description = "Light armor that provides basic protection.",
                Type = "Armor",
                Slot = "chest",
                Rarity = "common",
                ImageUrl = "/assets/items/leather-armor.png",
                StatBonuses = new Dictionary<string, int> { { "defense", 3 }, { "agility", 2 } },
                RequiredLevel = 1,
                RequiredClass = ""
            },
            new ItemTemplate
            {
                Id = 6,
                Name = "Chain Mail",
                Description = "Heavy armor made of interlocked metal rings.",
                Type = "Armor",
                Slot = "chest",
                Rarity = "uncommon",
                ImageUrl = "/assets/items/chain-mail.png",
                StatBonuses = new Dictionary<string, int> { { "defense", 8 } },
                RequiredLevel = 4,
                RequiredClass = "warrior"
            },
            new ItemTemplate
            {
                Id = 7,
                Name = "Robe of Power",
                Description = "A magical robe that enhances spellcasting.",
                Type = "Armor",
                Slot = "chest",
                Rarity = "rare",
                ImageUrl = "/assets/items/robe-of-power.png",
                StatBonuses = new Dictionary<string, int> { { "magic", 8 }, { "defense", 3 } },
                RequiredLevel = 6,
                RequiredClass = "mage"
            },

            // Accessories
            new ItemTemplate
            {
                Id = 8,
                Name = "Ring of Strength",
                Description = "A ring that increases physical power.",
                Type = "Accessory",
                Slot = "ring",
                Rarity = "common",
                ImageUrl = "/assets/items/ring-of-strength.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 3 } },
                RequiredLevel = 2,
                RequiredClass = ""
            },
            new ItemTemplate
            {
                Id = 9,
                Name = "Amulet of Wisdom",
                Description = "An amulet that enhances magical knowledge.",
                Type = "Accessory",
                Slot = "amulet",
                Rarity = "uncommon",
                ImageUrl = "/assets/items/amulet-of-wisdom.png",
                StatBonuses = new Dictionary<string, int> { { "magic", 5 }, { "agility", 2 } },
                RequiredLevel = 4,
                RequiredClass = ""
            },
            new ItemTemplate
            {
                Id = 10,
                Name = "Boots of Speed",
                Description = "Lightweight boots that increase movement speed.",
                Type = "Accessory",
                Slot = "feet",
                Rarity = "rare",
                ImageUrl = "/assets/items/boots-of-speed.png",
                StatBonuses = new Dictionary<string, int> { { "agility", 8 }, { "speed", 5 } },
                RequiredLevel = 7,
                RequiredClass = ""
            },

            // More Weapons
            new ItemTemplate
            {
                Id = 11,
                Name = "Dragon Slayer",
                Description = "A legendary blade forged to slay dragons.",
                Type = "Weapon",
                Slot = "weapon",
                Rarity = "legendary",
                ImageUrl = "/assets/items/dragon-slayer.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 25 }, { "magic", 10 } },
                RequiredLevel = 15,
                RequiredClass = "warrior"
            },
            new ItemTemplate
            {
                Id = 12,
                Name = "Shadow Blade",
                Description = "A dagger that strikes from the shadows.",
                Type = "Weapon",
                Slot = "weapon",
                Rarity = "epic",
                ImageUrl = "/assets/items/shadow-blade.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 15 }, { "agility", 12 } },
                RequiredLevel = 12,
                RequiredClass = "rogue"
            },
            new ItemTemplate
            {
                Id = 13,
                Name = "Arcane Staff",
                Description = "A staff imbued with ancient magic.",
                Type = "Weapon",
                Slot = "weapon",
                Rarity = "epic",
                ImageUrl = "/assets/items/arcane-staff.png",
                StatBonuses = new Dictionary<string, int> { { "magic", 20 }, { "defense", 5 } },
                RequiredLevel = 10,
                RequiredClass = "mage"
            },
            new ItemTemplate
            {
                Id = 14,
                Name = "Divine Hammer",
                Description = "A hammer blessed by the gods.",
                Type = "Weapon",
                Slot = "weapon",
                Rarity = "rare",
                ImageUrl = "/assets/items/divine-hammer.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 12 }, { "magic", 8 } },
                RequiredLevel = 8,
                RequiredClass = "paladin"
            },
            new ItemTemplate
            {
                Id = 15,
                Name = "Crystal Sword",
                Description = "A sword made from pure crystal.",
                Type = "Weapon",
                Slot = "weapon",
                Rarity = "uncommon",
                ImageUrl = "/assets/items/crystal-sword.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 8 }, { "magic", 4 } },
                RequiredLevel = 6,
                RequiredClass = ""
            },

            // More Armor
            new ItemTemplate
            {
                Id = 16,
                Name = "Plate Armor",
                Description = "Heavy armor made from thick metal plates.",
                Type = "Armor",
                Slot = "chest",
                Rarity = "uncommon",
                ImageUrl = "/assets/items/plate-armor.png",
                StatBonuses = new Dictionary<string, int> { { "defense", 12 } },
                RequiredLevel = 6,
                RequiredClass = "warrior"
            },
            new ItemTemplate
            {
                Id = 17,
                Name = "Mage Robes",
                Description = "Enchanted robes that enhance magical abilities.",
                Type = "Armor",
                Slot = "chest",
                Rarity = "common",
                ImageUrl = "/assets/items/mage-robes.png",
                StatBonuses = new Dictionary<string, int> { { "magic", 6 }, { "defense", 2 } },
                RequiredLevel = 3,
                RequiredClass = "mage"
            },
            new ItemTemplate
            {
                Id = 18,
                Name = "Leather Helmet",
                Description = "A simple leather helmet for basic protection.",
                Type = "Armor",
                Slot = "head",
                Rarity = "common",
                ImageUrl = "/assets/items/leather-helmet.png",
                StatBonuses = new Dictionary<string, int> { { "defense", 3 } },
                RequiredLevel = 2,
                RequiredClass = ""
            },
            new ItemTemplate
            {
                Id = 19,
                Name = "Steel Gauntlets",
                Description = "Heavy metal gauntlets that increase strength.",
                Type = "Armor",
                Slot = "arms",
                Rarity = "uncommon",
                ImageUrl = "/assets/items/steel-gauntlets.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 5 }, { "defense", 4 } },
                RequiredLevel = 5,
                RequiredClass = "warrior"
            },
            new ItemTemplate
            {
                Id = 20,
                Name = "Chain Leggings",
                Description = "Protective leggings made from chain mail.",
                Type = "Armor",
                Slot = "legs",
                Rarity = "common",
                ImageUrl = "/assets/items/chain-leggings.png",
                StatBonuses = new Dictionary<string, int> { { "defense", 5 } },
                RequiredLevel = 3,
                RequiredClass = ""
            },

            // More Accessories
            new ItemTemplate
            {
                Id = 21,
                Name = "Ring of Power",
                Description = "A ring that increases all magical abilities.",
                Type = "Accessory",
                Slot = "ring",
                Rarity = "epic",
                ImageUrl = "/assets/items/ring-of-power.png",
                StatBonuses = new Dictionary<string, int> { { "magic", 12 }, { "attack", 5 } },
                RequiredLevel = 10,
                RequiredClass = ""
            },
            new ItemTemplate
            {
                Id = 22,
                Name = "Amulet of Vitality",
                Description = "An amulet that increases health and endurance.",
                Type = "Accessory",
                Slot = "amulet",
                Rarity = "rare",
                ImageUrl = "/assets/items/amulet-of-vitality.png",
                StatBonuses = new Dictionary<string, int> { { "health", 20 }, { "defense", 6 } },
                RequiredLevel = 8,
                RequiredClass = ""
            },
            new ItemTemplate
            {
                Id = 23,
                Name = "Belt of Strength",
                Description = "A belt that enhances physical power.",
                Type = "Accessory",
                Slot = "belt",
                Rarity = "uncommon",
                ImageUrl = "/assets/items/belt-of-strength.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 8 } },
                RequiredLevel = 4,
                RequiredClass = ""
            },
            new ItemTemplate
            {
                Id = 24,
                Name = "Cape of Shadows",
                Description = "A mysterious cape that enhances stealth.",
                Type = "Accessory",
                Slot = "back",
                Rarity = "rare",
                ImageUrl = "/assets/items/cape-of-shadows.png",
                StatBonuses = new Dictionary<string, int> { { "agility", 10 }, { "stealth", 8 } },
                RequiredLevel = 9,
                RequiredClass = "rogue"
            },
            new ItemTemplate
            {
                Id = 25,
                Name = "Gloves of Dexterity",
                Description = "Gloves that improve hand-eye coordination.",
                Type = "Accessory",
                Slot = "hands",
                Rarity = "common",
                ImageUrl = "/assets/items/gloves-of-dexterity.png",
                StatBonuses = new Dictionary<string, int> { { "agility", 4 } },
                RequiredLevel = 2,
                RequiredClass = ""
            },

            // High-level Items
            new ItemTemplate
            {
                Id = 26,
                Name = "Excalibur",
                Description = "The legendary sword of kings.",
                Type = "Weapon",
                Slot = "weapon",
                Rarity = "legendary",
                ImageUrl = "/assets/items/excalibur.png",
                StatBonuses = new Dictionary<string, int> { { "attack", 30 }, { "magic", 15 }, { "defense", 10 } },
                RequiredLevel = 20,
                RequiredClass = "paladin"
            },
            new ItemTemplate
            {
                Id = 27,
                Name = "Archmage Robes",
                Description = "Robe worn by the most powerful mages.",
                Type = "Armor",
                Slot = "chest",
                Rarity = "legendary",
                ImageUrl = "/assets/items/archmage-robes.png",
                StatBonuses = new Dictionary<string, int> { { "magic", 25 }, { "defense", 8 }, { "health", 15 } },
                RequiredLevel = 18,
                RequiredClass = "mage"
            },
            new ItemTemplate
            {
                Id = 28,
                Name = "Assassin's Cloak",
                Description = "A cloak that makes its wearer nearly invisible.",
                Type = "Accessory",
                Slot = "back",
                Rarity = "legendary",
                ImageUrl = "/assets/items/assassins-cloak.png",
                StatBonuses = new Dictionary<string, int> { { "agility", 20 }, { "stealth", 15 }, { "attack", 10 } },
                RequiredLevel = 16,
                RequiredClass = "rogue"
            },
            new ItemTemplate
            {
                Id = 29,
                Name = "Titan's Plate",
                Description = "Armor forged by the gods themselves.",
                Type = "Armor",
                Slot = "chest",
                Rarity = "legendary",
                ImageUrl = "/assets/items/titans-plate.png",
                StatBonuses = new Dictionary<string, int> { { "defense", 30 }, { "health", 25 }, { "attack", 5 } },
                RequiredLevel = 22,
                RequiredClass = "warrior"
            },
            new ItemTemplate
            {
                Id = 30,
                Name = "Crown of Kings",
                Description = "A crown that grants wisdom and power.",
                Type = "Accessory",
                Slot = "head",
                Rarity = "legendary",
                ImageUrl = "/assets/items/crown-of-kings.png",
                StatBonuses = new Dictionary<string, int> { { "magic", 20 }, { "attack", 15 }, { "defense", 12 } },
                RequiredLevel = 25,
                RequiredClass = ""
            }
        };
    }
}
