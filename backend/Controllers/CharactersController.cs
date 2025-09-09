using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Linq;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/characters")]
    [Authorize]
    public class CharactersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CharactersController(AppDbContext db)
        {
            _db = db;
        }

        // ======= Equipment management =======
        public class EquipItemDto
        {
            public string Slot { get; set; } = string.Empty; // e.g., "head", "weapon", "chest"
            public int ItemId { get; set; } // template Id
        }

        [HttpPost("equipment/equip")]
        public async Task<IActionResult> EquipItem([FromBody] EquipItemDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Slot))
                return BadRequest(new { message = "Slot is required" });

            var chr = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
            if (chr == null)
                return NotFound();

            // Parse inventory and equipment
            var inventory = new List<System.Text.Json.Nodes.JsonObject>();
            if (!string.IsNullOrWhiteSpace(chr.InventoryJson))
            {
                try
                {
                    var arr = System.Text.Json.Nodes.JsonNode.Parse(chr.InventoryJson)?.AsArray();
                    if (arr != null)
                    {
                        foreach (var node in arr)
                        {
                            if (node is System.Text.Json.Nodes.JsonObject obj)
                                inventory.Add(obj);
                        }
                    }
                }
                catch { }
            }

            // Validate item exists in inventory and matches slot
            var matchingItem = inventory.FirstOrDefault(o =>
                (o["Id"]?.GetValue<int>() ?? 0) == dto.ItemId &&
                string.Equals((o["Slot"]?.GetValue<string>() ?? (o["slot"]?.GetValue<string>() ?? "")).ToString(), dto.Slot, StringComparison.OrdinalIgnoreCase)
            );

            if (matchingItem == null)
                return BadRequest(new { message = "Item not found in inventory for this slot" });

            // Update equipment map (array of { slot, itemId })
            var equipment = new List<System.Text.Json.Nodes.JsonObject>();
            if (!string.IsNullOrWhiteSpace(chr.EquipmentJson))
            {
                try
                {
                    var arr = System.Text.Json.Nodes.JsonNode.Parse(chr.EquipmentJson)?.AsArray();
                    if (arr != null)
                    {
                        foreach (var node in arr)
                        {
                            if (node is System.Text.Json.Nodes.JsonObject obj)
                                equipment.Add(obj);
                        }
                    }
                }
                catch { }
            }

            // Remove existing for slot
            equipment.RemoveAll(e => string.Equals((e["slot"]?.GetValue<string>() ?? (e["Slot"]?.GetValue<string>() ?? "")).ToString(), dto.Slot, StringComparison.OrdinalIgnoreCase));
            // Add new
            var newEquip = new System.Text.Json.Nodes.JsonObject
            {
                ["slot"] = dto.Slot.ToLower(),
                ["itemId"] = dto.ItemId
            };
            equipment.Add(newEquip);

            chr.EquipmentJson = System.Text.Json.JsonSerializer.Serialize(equipment);
            chr.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                chr.Id,
                chr.EquipmentJson
            });
        }

        public class UnequipItemDto
        {
            public string Slot { get; set; } = string.Empty;
        }

        [HttpPost("equipment/unequip")]
        public async Task<IActionResult> UnequipItem([FromBody] UnequipItemDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Slot))
                return BadRequest(new { message = "Slot is required" });

            var chr = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
            if (chr == null)
                return NotFound();

            var equipment = new List<System.Text.Json.Nodes.JsonObject>();
            if (!string.IsNullOrWhiteSpace(chr.EquipmentJson))
            {
                try
                {
                    var arr = System.Text.Json.Nodes.JsonNode.Parse(chr.EquipmentJson)?.AsArray();
                    if (arr != null)
                    {
                        foreach (var node in arr)
                        {
                            if (node is System.Text.Json.Nodes.JsonObject obj)
                                equipment.Add(obj);
                        }
                    }
                }
                catch { }
            }

            int removed = equipment.RemoveAll(e => string.Equals((e["slot"]?.GetValue<string>() ?? (e["Slot"]?.GetValue<string>() ?? "")).ToString(), dto.Slot, StringComparison.OrdinalIgnoreCase));
            if (removed == 0)
                return Ok(new { message = "Nothing equipped in this slot" });

            chr.EquipmentJson = System.Text.Json.JsonSerializer.Serialize(equipment);
            chr.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Unequipped", chr.EquipmentJson });
        }

        // ======= Equip abilities (mark 4 as equipped in AttacksJson) =======
        public class EquipAbilitiesDto
        {
            public List<int> AttackIds { get; set; } = new(); // up to 4 ids
        }

        [HttpPost("attacks/equip")]
        public async Task<IActionResult> EquipAbilities([FromBody] EquipAbilitiesDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var chr = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
            if (chr == null)
                return NotFound();

            if (string.IsNullOrEmpty(chr.AttacksJson))
                return BadRequest(new { message = "No attacks owned" });

            var arr = System.Text.Json.Nodes.JsonNode.Parse(chr.AttacksJson)?.AsArray();
            if (arr == null)
                return BadRequest(new { message = "Invalid attacks data" });

            // Limit to 4 unique ids
            var toEquip = dto.AttackIds.Distinct().Take(4).ToHashSet();

            foreach (var node in arr)
            {
                if (node is System.Text.Json.Nodes.JsonObject obj)
                {
                    int id = 0;
                    if (obj.ContainsKey("Id")) id = obj["Id"]?.GetValue<int>() ?? 0;
                    else if (obj.ContainsKey("id")) id = obj["id"]?.GetValue<int>() ?? 0;
                    obj["Equipped"] = toEquip.Contains(id);
                }
            }

            chr.AttacksJson = System.Text.Json.JsonSerializer.Serialize(arr);
            chr.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { chr.AttacksJson });
        }

        [HttpPut("recharge")]
        public async Task<IActionResult> RechargeCharacter()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var chr = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
            if (chr == null)
                return NotFound();

            var now = DateTime.UtcNow;
            var elapsedSeconds = (now - chr.LastRechargeTime).TotalSeconds;

            const int energyInterval = 120;
            int ticks = (int)(elapsedSeconds / energyInterval);

            if (ticks > 0)
            {
                int energyToAdd = Math.Min(ticks, chr.MaxEnergy - chr.CurrentEnergy);
                int healthToAdd = Math.Min(ticks * 10, chr.MaxHealth - chr.CurrentHealth);

                chr.CurrentEnergy += energyToAdd;
                chr.CurrentHealth += healthToAdd;
                if (chr.CurrentEnergy > chr.MaxEnergy)
                    chr.CurrentEnergy = chr.MaxEnergy;
                if (chr.CurrentHealth > chr.MaxHealth)
                    chr.CurrentHealth = chr.MaxHealth;

                chr.LastRechargeTime = chr.LastRechargeTime.AddSeconds(ticks * energyInterval);

                if (!string.IsNullOrEmpty(chr.AttacksJson))
                {
                    var attacks = JsonSerializer.Deserialize<List<AttackData>>(chr.AttacksJson) ?? new List<AttackData>();
                    foreach (var atk in attacks)
                    {
                        atk.CurrentCharges = atk.MaxCharges;
                    }
                    chr.AttacksJson = JsonSerializer.Serialize(attacks);
                }

                await _db.SaveChangesAsync();
            }

            var nextTickInSeconds = energyInterval - ((int)elapsedSeconds % energyInterval);

            return Ok(new
            {
                chr.Id,
                chr.Name,
                chr.Class,
                chr.ProfileIconUrl,
                chr.Level,
                chr.ExperiencePoints,
                chr.MaxExperiencePoints,
                chr.CurrentHealth,
                chr.MaxHealth,
                chr.CurrentEnergy,
                chr.MaxEnergy,
                chr.Attack,
                chr.Defense,
                chr.Agility,
                chr.Magic,
                chr.Speed,
                chr.CriticalChance,
                chr.UnspentStatPoints,
                canAllocateStats = chr.UnspentStatPoints >= 5,
                chr.Credits,
                chr.InventoryJson,
                chr.EquipmentJson,
                chr.CreatedAt,
                chr.UpdatedAt,
                chr.LastRechargeTime,
                nextTickInSeconds,
                chr.AttacksJson
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyCharacter()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var chr = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);

            if (chr == null)
                return NotFound();

            const int energyInterval = 120;
            var now = DateTime.UtcNow;
            var elapsedSeconds = (now - chr.LastRechargeTime).TotalSeconds;
            var nextTickInSeconds = energyInterval - ((int)elapsedSeconds % energyInterval);
            if (nextTickInSeconds <= 0) nextTickInSeconds = energyInterval;

            return Ok(new
            {
                chr.Id,
                chr.Name,
                chr.Class,
                chr.ProfileIconUrl,
                chr.Level,
                chr.ExperiencePoints,
                chr.MaxExperiencePoints,
                chr.CurrentHealth,
                chr.MaxHealth,
                chr.CurrentEnergy,
                chr.MaxEnergy,
                chr.Attack,
                chr.Defense,
                chr.Agility,
                chr.Magic,
                chr.Speed,
                chr.CriticalChance,
                chr.UnspentStatPoints,
                canAllocateStats = chr.UnspentStatPoints >= 5,
                chr.Credits,
                chr.InventoryJson,
                chr.EquipmentJson,
                chr.CreatedAt,
                chr.UpdatedAt,
                chr.LastRechargeTime,
                nextTickInSeconds,
                chr.AttacksJson
            });
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetCharacterByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { message = "Username is required" });

            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
                return NotFound();

            var chr = await _db.Characters
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (chr == null)
                return NotFound();

            const int energyInterval = 120;
            var now = DateTime.UtcNow;
            var elapsedSeconds = (now - chr.LastRechargeTime).TotalSeconds;
            var nextTickInSeconds = energyInterval - ((int)elapsedSeconds % energyInterval);
            if (nextTickInSeconds <= 0) nextTickInSeconds = energyInterval;

            return Ok(new
            {
                chr.Id,
                chr.Name,
                chr.Class,
                chr.ProfileIconUrl,
                chr.Level,
                chr.ExperiencePoints,
                chr.MaxExperiencePoints,
                chr.CurrentHealth,
                chr.MaxHealth,
                chr.CurrentEnergy,
                chr.MaxEnergy,
                chr.Attack,
                chr.Defense,
                chr.Agility,
                chr.Magic,
                chr.Speed,
                chr.CriticalChance,
                chr.UnspentStatPoints,
                canAllocateStats = chr.UnspentStatPoints >= 5,
                chr.Credits,
                chr.InventoryJson,
                chr.EquipmentJson,
                chr.CreatedAt,
                chr.UpdatedAt,
                chr.LastRechargeTime,
                nextTickInSeconds,
                chr.AttacksJson
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCharacter(CreateCharacterDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            if (await _db.Characters.AnyAsync(c => c.UserId == userId))
                return BadRequest("User already has a character.");

            var chr = new Character
            {
                UserId = userId,
                Name = dto.Name,
                Class = dto.Class,
                ProfileIconUrl = dto.ProfileIconUrl,
                LastRechargeTime = DateTime.UtcNow,
            };

            var klass = dto.Class?.Trim().ToLower() ?? "";
            
            // Give only the first (basic) attack for each class
            var basicAttack = AttackTemplates.All
                .Where(atk => atk.AllowedClasses.Any(ac => ac.ToLower() == klass))
                .OrderBy(atk => atk.Id) // First attack is usually the basic one
                .FirstOrDefault();

            if (basicAttack != null)
            {
                var startingAttack = new
                {
                    basicAttack.Id,
                    basicAttack.Name,
                    basicAttack.Type,
                    basicAttack.DamageType,
                    basicAttack.BaseDamage,
                    basicAttack.MaxCharges,
                    CurrentCharges = basicAttack.MaxCharges,
                    basicAttack.Scaling,
                    basicAttack.RequiredStats,
                    basicAttack.AllowedClasses,
                    basicAttack.Description,
                    basicAttack.HealAmount,
                    basicAttack.BlockNextAttack,
                    basicAttack.Poison,
                    basicAttack.EvadeNextAttack,
                    basicAttack.CritChanceBonus,
                    basicAttack.CritBonusTurns,
                    basicAttack.PoisonDamagePerTurn,
                    basicAttack.PoisonDuration
                };

                chr.AttacksJson = JsonSerializer.Serialize(new[] { startingAttack });
            }
            else
            {
                chr.AttacksJson = "[]";
            }

            _db.Characters.Add(chr);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMyCharacter),
                null,
                new
                {
                    chr.Id,
                    chr.Name,
                    chr.Class,
                    chr.ProfileIconUrl,
                    chr.Level,
                    chr.ExperiencePoints,
                    chr.MaxExperiencePoints,
                    chr.CurrentHealth,
                    chr.MaxHealth,
                    chr.CurrentEnergy,
                    chr.MaxEnergy,
                    chr.Attack,
                    chr.Defense,
                    chr.Agility,
                    chr.Magic,
                    chr.Speed,
                    chr.CriticalChance,
                    chr.UnspentStatPoints,
                    canAllocateStats = chr.UnspentStatPoints >= 5,
                    chr.Credits,
                    chr.InventoryJson,
                    chr.EquipmentJson,
                    chr.CreatedAt,
                    chr.UpdatedAt,
                    chr.LastRechargeTime,
                    chr.AttacksJson
                }
            );
        }

        // ======= Level-up stats =======
        public class UpdateStatsDto
        {
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int Agility { get; set; }
            public int Magic { get; set; }
            public int Speed { get; set; }
            public int MaxHealth { get; set; } 
        }

        [HttpPatch("stats")]
        public async Task<IActionResult> UpdateStats([FromBody] UpdateStatsDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var chr = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
            if (chr == null)
                return NotFound();

            if (chr.UnspentStatPoints <= 0)
                return BadRequest(new { message = "No stat points available." });

            const int HpPerPoint = 5;

            int deltaAttack = dto.Attack - chr.Attack;
            int deltaDefense = dto.Defense - chr.Defense;
            int deltaAgility = dto.Agility - chr.Agility;
            int deltaMagic  = dto.Magic  - chr.Magic;
            int deltaSpeed  = dto.Speed  - chr.Speed;

            if (deltaAttack < 0 || deltaDefense < 0 || deltaAgility < 0 || deltaMagic < 0 || deltaSpeed < 0)
                return BadRequest(new { message = "Stats can only be increased." });

            if (dto.MaxHealth < chr.MaxHealth)
                return BadRequest(new { message = "HP can only be increased." });

            int hpDiff = dto.MaxHealth - chr.MaxHealth;
            if (hpDiff % HpPerPoint != 0)
                return BadRequest(new { message = $"HP must be increased in steps of {HpPerPoint}." });

            int hpPoints = hpDiff / HpPerPoint;

            int totalPoints = deltaAttack + deltaDefense + deltaAgility + deltaMagic + deltaSpeed + hpPoints;

            if (totalPoints != chr.UnspentStatPoints)
                return BadRequest(new { message = $"You must allocate exactly all your unspent points ({chr.UnspentStatPoints})." });

            // Apply
            chr.Attack    = dto.Attack;
            chr.Defense   = dto.Defense;
            chr.Agility   = dto.Agility;
            chr.Magic     = dto.Magic;
            chr.Speed     = dto.Speed;
            chr.MaxHealth = dto.MaxHealth;

            chr.UnspentStatPoints -= totalPoints; 
            chr.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Response 
            const int energyInterval = 120;
            var now = DateTime.UtcNow;
            var elapsedSeconds = (now - chr.LastRechargeTime).TotalSeconds;
            var nextTickInSeconds = energyInterval - ((int)elapsedSeconds % energyInterval);
            if (nextTickInSeconds <= 0) nextTickInSeconds = energyInterval;

            return Ok(new
            {
                chr.Id,
                chr.Name,
                chr.Class,
                chr.ProfileIconUrl,
                chr.Level,
                chr.ExperiencePoints,
                chr.MaxExperiencePoints,
                chr.CurrentHealth,
                chr.MaxHealth,
                chr.CurrentEnergy,
                chr.MaxEnergy,
                chr.Attack,
                chr.Defense,
                chr.Agility,
                chr.Magic,
                chr.Speed,
                chr.CriticalChance,
                chr.UnspentStatPoints,
                canAllocateStats = chr.UnspentStatPoints >= 5,
                chr.Credits,
                chr.InventoryJson,
                chr.EquipmentJson,
                chr.CreatedAt,
                chr.UpdatedAt,
                chr.LastRechargeTime,
                nextTickInSeconds,
                chr.AttacksJson
            });
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteCharacter()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var chr = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
            if (chr == null)
                return NotFound();

            _db.Characters.Remove(chr);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("use-energy")]
        public async Task<IActionResult> UseEnergy([FromBody] UseEnergyDto? dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var chr = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
            if (chr == null)
                return NotFound();

            int amount = dto?.Amount ?? 1;
            if (amount <= 0)
                amount = 1;

            if (chr.CurrentEnergy < amount)
                return BadRequest(new { message = "Not enough energy" });

            chr.CurrentEnergy -= amount;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                chr.CurrentEnergy
            });
        }
    }

    public class UseEnergyDto
    {
        public int Amount { get; set; } = 1;
    }

    public class CreateCharacterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string ProfileIconUrl { get; set; } = string.Empty;
    }

    public class AttackData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? DamageType { get; set; }
        public int BaseDamage { get; set; }
        public int MaxCharges { get; set; }
        public int CurrentCharges { get; set; }
        public Dictionary<string, double>? Scaling { get; set; }
        public Dictionary<string, int>? RequiredStats { get; set; }
        public List<string>? AllowedClasses { get; set; }
        public string? Description { get; set; }
    }
}
