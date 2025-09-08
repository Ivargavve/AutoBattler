using backend.Data;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/item-shop")]
    [Authorize]
    public class ItemShopController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ItemShopController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableItems()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized();

                var character = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
                if (character == null)
                    return NotFound("Character not found");

                // Get all items, but mark which ones are available
                var availableItems = ItemTemplates.All
                    .Select(item => new
                    {
                        item.Id,
                        item.Name,
                        item.Description,
                        item.Type,
                        item.Slot,
                        item.Rarity,
                        item.ImageUrl,
                        item.StatBonuses,
                        item.RequiredLevel,
                        item.RequiredClass,
                        Price = CalculateItemPrice(item),
                        CanAfford = character.Credits >= CalculateItemPrice(item),
                        MeetsRequirements = MeetsItemRequirements(character, item),
                        IsAvailable = item.RequiredLevel <= character.Level
                    })
                    .OrderBy(item => item.Price)
                    .ThenBy(item => item.Name)
                    .ToList();

                return Ok(availableItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving available items", error = ex.Message });
            }
        }

        [HttpPost("purchase/{itemId}")]
        public async Task<IActionResult> PurchaseItem(int itemId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized();

                var character = await _db.Characters.FirstOrDefaultAsync(c => c.UserId == userId);
                if (character == null)
                    return NotFound("Character not found");

                var itemTemplate = ItemTemplates.All.FirstOrDefault(item => item.Id == itemId);
                if (itemTemplate == null)
                    return NotFound("Item not found");

                // Check requirements
                if (!MeetsItemRequirements(character, itemTemplate))
                    return BadRequest("You don't meet the requirements for this item");

                // Check if character can afford it
                var price = CalculateItemPrice(itemTemplate);
                if (character.Credits < price)
                    return BadRequest("Not enough credits");

                // Purchase the item
                character.Credits -= price;

                // Add item to character's inventory
                var inventory = new List<object>();
                if (!string.IsNullOrEmpty(character.InventoryJson))
                {
                    try
                    {
                        var existingInventory = JsonSerializer.Deserialize<List<dynamic>>(character.InventoryJson);
                        if (existingInventory != null)
                        {
                            inventory.AddRange(existingInventory.Cast<object>());
                        }
                    }
                    catch
                    {
                        // If parsing fails, start fresh
                    }
                }

                var newItem = new
                {
                    itemTemplate.Id,
                    itemTemplate.Name,
                    itemTemplate.Description,
                    itemTemplate.Type,
                    itemTemplate.Slot,
                    itemTemplate.Rarity,
                    itemTemplate.ImageUrl,
                    itemTemplate.StatBonuses,
                    itemTemplate.RequiredLevel,
                    itemTemplate.RequiredClass,
                    Quantity = 1,
                    PurchasedAt = DateTime.UtcNow
                };

                inventory.Add(newItem);
                character.InventoryJson = JsonSerializer.Serialize(inventory);

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Item purchased successfully",
                    remainingCredits = character.Credits,
                    item = newItem
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error purchasing item", error = ex.Message });
            }
        }

        private int CalculateItemPrice(ItemTemplate item)
        {
            // Base price calculation based on item stats and rarity
            var basePrice = 100;
            var statBonus = item.StatBonuses.Values.Sum() * 25;
            var rarityMultiplier = item.Rarity switch
            {
                "common" => 1.0,
                "uncommon" => 1.5,
                "rare" => 2.0,
                "epic" => 3.0,
                "legendary" => 5.0,
                _ => 1.0
            };

            return (int)((basePrice + statBonus) * rarityMultiplier);
        }

        private bool MeetsItemRequirements(Character character, ItemTemplate item)
        {
            if (character.Level < item.RequiredLevel)
                return false;

            if (!string.IsNullOrEmpty(item.RequiredClass) && 
                !character.Class.ToLower().Contains(item.RequiredClass.ToLower()))
                return false;

            return true;
        }
    }

}
