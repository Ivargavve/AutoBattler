using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
                chr.CurrentHealth,
                chr.MaxHealth,
                chr.CurrentEnergy,
                chr.MaxEnergy,
                chr.Attack,
                chr.Defense,
                chr.Agility,
                chr.CriticalChance,
                chr.Credits,
                chr.InventoryJson,
                chr.EquipmentJson,
                chr.CreatedAt,
                chr.UpdatedAt,
                chr.LastRechargeTime,
                nextTickInSeconds
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
                chr.CurrentHealth,
                chr.MaxHealth,
                chr.CurrentEnergy,
                chr.MaxEnergy,
                chr.Attack,
                chr.Defense,
                chr.Agility,
                chr.CriticalChance,
                chr.Credits,
                chr.InventoryJson,
                chr.EquipmentJson,
                chr.CreatedAt,
                chr.UpdatedAt,
                chr.LastRechargeTime,
                nextTickInSeconds
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
                chr.CurrentHealth,
                chr.MaxHealth,
                chr.CurrentEnergy,
                chr.MaxEnergy,
                chr.Attack,
                chr.Defense,
                chr.Agility,
                chr.CriticalChance,
                chr.Credits,
                chr.InventoryJson,
                chr.EquipmentJson,
                chr.CreatedAt,
                chr.UpdatedAt,
                chr.LastRechargeTime,
                nextTickInSeconds
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
                LastRechargeTime = DateTime.UtcNow
            };

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
                    chr.CurrentHealth,
                    chr.MaxHealth,
                    chr.CurrentEnergy,
                    chr.MaxEnergy,
                    chr.Attack,
                    chr.Defense,
                    chr.Agility,
                    chr.CriticalChance,
                    chr.Credits,
                    chr.InventoryJson,
                    chr.EquipmentJson,
                    chr.CreatedAt,
                    chr.UpdatedAt,
                    chr.LastRechargeTime
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
}
