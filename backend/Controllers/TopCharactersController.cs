using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/top-characters")]
    public class TopCharactersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TopCharactersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetTopCharacters()
        {
            try
            {
                var topByLevel = await GetTopCharactersByLevel(5);
                var topByAttack = await GetTopCharactersByAttack(5);
                var topByHealth = await GetTopCharactersByHealth(5);
                var topByDefense = await GetTopCharactersByDefense(5);
                var topByAgility = await GetTopCharactersByAgility(5);
                var topByMagic = await GetTopCharactersByMagic(5);

                return Ok(new
                {
                    kingOfAutobattler = topByLevel,
                    attackMasters = topByAttack,
                    tankyBankies = topByHealth,
                    defenseChampions = topByDefense,
                    speedDemons = topByAgility,
                    magicWielders = topByMagic
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving top characters data", error = ex.Message });
            }
        }

        private async Task<List<TopCharacterEntry>> GetTopCharactersByLevel(int count)
        {
            return await _db.Characters
                .Include(c => c.User)
                .OrderByDescending(c => c.Level)
                .ThenByDescending(c => c.ExperiencePoints)
                .Take(count)
                .Select(c => new TopCharacterEntry
                {
                    CharacterName = c.Name,
                    UserName = c.User!.Username,
                    Class = c.Class,
                    Level = c.Level,
                    StatValue = c.Level,
                    StatName = "Level",
                    ProfileIconUrl = c.ProfileIconUrl,
                    Title = c.Level >= 50 ? "King of Autobattler" : 
                           c.Level >= 30 ? "Legendary Warrior" : 
                           c.Level >= 20 ? "Veteran Fighter" : "Rising Star"
                })
                .ToListAsync();
        }

        private async Task<List<TopCharacterEntry>> GetTopCharactersByAttack(int count)
        {
            return await _db.Characters
                .Include(c => c.User)
                .OrderByDescending(c => c.Attack)
                .ThenByDescending(c => c.Level)
                .Take(count)
                .Select(c => new TopCharacterEntry
                {
                    CharacterName = c.Name,
                    UserName = c.User!.Username,
                    Class = c.Class,
                    Level = c.Level,
                    StatValue = c.Attack,
                    StatName = "Attack",
                    ProfileIconUrl = c.ProfileIconUrl,
                    Title = c.Attack >= 100 ? "Attack Master" : 
                           c.Attack >= 75 ? "Berserker" : 
                           c.Attack >= 50 ? "Warrior" : "Fighter"
                })
                .ToListAsync();
        }

        private async Task<List<TopCharacterEntry>> GetTopCharactersByHealth(int count)
        {
            return await _db.Characters
                .Include(c => c.User)
                .OrderByDescending(c => c.MaxHealth)
                .ThenByDescending(c => c.Level)
                .Take(count)
                .Select(c => new TopCharacterEntry
                {
                    CharacterName = c.Name,
                    UserName = c.User!.Username,
                    Class = c.Class,
                    Level = c.Level,
                    StatValue = c.MaxHealth,
                    StatName = "Health",
                    ProfileIconUrl = c.ProfileIconUrl,
                    Title = c.MaxHealth >= 500 ? "Tanky Banky" : 
                           c.MaxHealth >= 300 ? "Fortress" : 
                           c.MaxHealth >= 200 ? "Guardian" : "Protector"
                })
                .ToListAsync();
        }

        private async Task<List<TopCharacterEntry>> GetTopCharactersByDefense(int count)
        {
            return await _db.Characters
                .Include(c => c.User)
                .OrderByDescending(c => c.Defense)
                .ThenByDescending(c => c.Level)
                .Take(count)
                .Select(c => new TopCharacterEntry
                {
                    CharacterName = c.Name,
                    UserName = c.User!.Username,
                    Class = c.Class,
                    Level = c.Level,
                    StatValue = c.Defense,
                    StatName = "Defense",
                    ProfileIconUrl = c.ProfileIconUrl,
                    Title = c.Defense >= 80 ? "Defense Champion" : 
                           c.Defense >= 60 ? "Shield Master" : 
                           c.Defense >= 40 ? "Guardian" : "Defender"
                })
                .ToListAsync();
        }

        private async Task<List<TopCharacterEntry>> GetTopCharactersByAgility(int count)
        {
            return await _db.Characters
                .Include(c => c.User)
                .OrderByDescending(c => c.Agility)
                .ThenByDescending(c => c.Level)
                .Take(count)
                .Select(c => new TopCharacterEntry
                {
                    CharacterName = c.Name,
                    UserName = c.User!.Username,
                    Class = c.Class,
                    Level = c.Level,
                    StatValue = c.Agility,
                    StatName = "Agility",
                    ProfileIconUrl = c.ProfileIconUrl,
                    Title = c.Agility >= 60 ? "Speed Demon" : 
                           c.Agility >= 45 ? "Lightning" : 
                           c.Agility >= 30 ? "Swift" : "Quick"
                })
                .ToListAsync();
        }

        private async Task<List<TopCharacterEntry>> GetTopCharactersByMagic(int count)
        {
            return await _db.Characters
                .Include(c => c.User)
                .OrderByDescending(c => c.Magic)
                .ThenByDescending(c => c.Level)
                .Take(count)
                .Select(c => new TopCharacterEntry
                {
                    CharacterName = c.Name,
                    UserName = c.User!.Username,
                    Class = c.Class,
                    Level = c.Level,
                    StatValue = c.Magic,
                    StatName = "Magic",
                    ProfileIconUrl = c.ProfileIconUrl,
                    Title = c.Magic >= 50 ? "Magic Wielder" : 
                           c.Magic >= 35 ? "Spellcaster" : 
                           c.Magic >= 20 ? "Mage" : "Apprentice"
                })
                .ToListAsync();
        }
    }

    public class TopCharacterEntry
    {
        public string CharacterName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public int Level { get; set; }
        public int StatValue { get; set; }
        public string StatName { get; set; } = string.Empty;
        public string ProfileIconUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
