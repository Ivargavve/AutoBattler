using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/attack-shop")]
    [Authorize]
    public class AttackShopController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AttackShopController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableAttacks()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized();

                var character = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
                if (character == null)
                    return NotFound("Character not found");

                // Get character's current attacks
                var currentAttacks = new List<int>();
                if (!string.IsNullOrEmpty(character.AttacksJson))
                {
                    try
                    {
                        var attacks = JsonSerializer.Deserialize<List<dynamic>>(character.AttacksJson);
                        if (attacks != null)
                        {
                            currentAttacks = attacks.Select(a => (int)a.GetProperty("Id").GetInt32()).ToList();
                        }
                    }
                    catch
                    {
                        // If parsing fails, assume no attacks
                    }
                }

                // Get all attacks, but mark which ones are available
                var availableAttacks = AttackTemplates.All
                    .Select(atk => new
                    {
                        atk.Id,
                        atk.Name,
                        atk.Type,
                        atk.DamageType,
                        atk.BaseDamage,
                        atk.MaxCharges,
                        atk.Scaling,
                        atk.RequiredStats,
                        atk.AllowedClasses,
                        atk.Description,
                        atk.HealAmount,
                        atk.BlockNextAttack,
                        atk.Poison,
                        atk.EvadeNextAttack,
                        atk.CritChanceBonus,
                        atk.CritBonusTurns,
                        atk.PoisonDamagePerTurn,
                        atk.PoisonDuration,
                        Price = CalculateAttackPrice(atk),
                        CanAfford = character.Credits >= CalculateAttackPrice(atk),
                        MeetsRequirements = MeetsRequirements(character, atk.RequiredStats),
                        IsAvailable = atk.AllowedClasses.Any(ac => ac.ToLower() == character.Class.ToLower()) && !currentAttacks.Contains(atk.Id)
                    })
                    .OrderBy(atk => atk.Price)
                    .ThenBy(atk => atk.Name)
                    .ToList();

                return Ok(availableAttacks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving available attacks", error = ex.Message });
            }
        }

        [HttpPost("purchase/{attackId}")]
        public async Task<IActionResult> PurchaseAttack(int attackId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized();

                var character = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
                if (character == null)
                    return NotFound("Character not found");

                var attackTemplate = AttackTemplates.All.FirstOrDefault(atk => atk.Id == attackId);
                if (attackTemplate == null)
                    return NotFound("Attack not found");

                // Check if character's class can use this attack
                if (!attackTemplate.AllowedClasses.Any(ac => ac.ToLower() == character.Class.ToLower()))
                    return BadRequest("Your class cannot use this attack");

                // Check if character already has this attack
                var currentAttacks = new List<int>();
                if (!string.IsNullOrEmpty(character.AttacksJson))
                {
                    try
                    {
                        var attacks = JsonSerializer.Deserialize<List<dynamic>>(character.AttacksJson);
                        if (attacks != null)
                        {
                            currentAttacks = attacks.Select(a => (int)a.GetProperty("Id").GetInt32()).ToList();
                        }
                    }
                    catch
                    {
                        // If parsing fails, assume no attacks
                    }
                }

                if (currentAttacks.Contains(attackId))
                    return BadRequest("You already have this attack");

                // Check requirements
                if (!MeetsRequirements(character, attackTemplate.RequiredStats))
                    return BadRequest("You don't meet the requirements for this attack");

                // Check if character can afford it
                var price = CalculateAttackPrice(attackTemplate);
                if (character.Credits < price)
                    return BadRequest("Not enough credits");

                // Purchase the attack
                character.Credits -= price;

                // Add attack to character
                var newAttack = new
                {
                    attackTemplate.Id,
                    attackTemplate.Name,
                    attackTemplate.Type,
                    attackTemplate.DamageType,
                    attackTemplate.BaseDamage,
                    attackTemplate.MaxCharges,
                    CurrentCharges = attackTemplate.MaxCharges,
                    attackTemplate.Scaling,
                    attackTemplate.RequiredStats,
                    attackTemplate.AllowedClasses,
                    attackTemplate.Description,
                    attackTemplate.HealAmount,
                    attackTemplate.BlockNextAttack,
                    attackTemplate.Poison,
                    attackTemplate.EvadeNextAttack,
                    attackTemplate.CritChanceBonus,
                    attackTemplate.CritBonusTurns,
                    attackTemplate.PoisonDamagePerTurn,
                    attackTemplate.PoisonDuration
                };

                var allAttacks = new List<object>();
                if (!string.IsNullOrEmpty(character.AttacksJson))
                {
                    try
                    {
                        var existingAttacks = JsonSerializer.Deserialize<List<dynamic>>(character.AttacksJson);
                        if (existingAttacks != null)
                        {
                            allAttacks.AddRange(existingAttacks.Cast<object>());
                        }
                    }
                    catch
                    {
                        // If parsing fails, start fresh
                    }
                }

                allAttacks.Add(newAttack);
                character.AttacksJson = JsonSerializer.Serialize(allAttacks);

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Attack purchased successfully",
                    remainingCredits = character.Credits,
                    attack = newAttack
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error purchasing attack", error = ex.Message });
            }
        }

        private int CalculateAttackPrice(AttackTemplate attack)
        {
            // Base price calculation based on attack stats
            var basePrice = attack.BaseDamage * 10;
            var scalingBonus = attack.Scaling.Values.Sum() * 50;
            var specialBonus = 0;

            if (attack.HealAmount > 0) specialBonus += 100;
            if (attack.BlockNextAttack) specialBonus += 75;
            if (attack.Poison) specialBonus += 50;
            if (attack.EvadeNextAttack) specialBonus += 100;
            if (attack.CritChanceBonus > 0) specialBonus += 75;

            return (int)(basePrice + scalingBonus + specialBonus);
        }

        private bool MeetsRequirements(Character character, Dictionary<string, int> requirements)
        {
            foreach (var req in requirements)
            {
                var statValue = req.Key.ToLower() switch
                {
                    "attack" => character.Attack,
                    "defense" => character.Defense,
                    "agility" => character.Agility,
                    "magic" => character.Magic,
                    "speed" => character.Speed,
                    _ => 0
                };

                if (statValue < req.Value)
                    return false;
            }
            return true;
        }
    }
}
