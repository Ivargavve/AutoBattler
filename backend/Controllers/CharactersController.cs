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
                // UnspentStatPoints startar på 0; Magic/Speed på 0 i modellen
            };

            var klass = dto.Class?.Trim().ToLower() ?? "";
            var matchingAttacks = AttackTemplates.All
                .Where(atk => atk.AllowedClasses.Any(ac => ac.ToLower() == klass))
                .Select(atk => new
                {
                    atk.Id,
                    atk.Name,
                    atk.Type,
                    atk.DamageType,
                    atk.BaseDamage,
                    atk.MaxCharges,
                    CurrentCharges = atk.MaxCharges,
                    atk.Scaling,
                    atk.RequiredStats,
                    atk.AllowedClasses,
                    atk.Description
                })
                .ToList();

            chr.AttacksJson = JsonSerializer.Serialize(matchingAttacks);

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

        [HttpPut]
        public async Task<IActionResult> UpdateCharacter(UpdateCharacterDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var chr = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
            if (chr == null)
                return NotFound();

            chr.Name = dto.Name;
            chr.ProfileIconUrl = dto.ProfileIconUrl;
            chr.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ======= Level-up stats (no crit here) =======
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

    public class UpdateCharacterDto
    {
        public string Name { get; set; } = string.Empty;
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
