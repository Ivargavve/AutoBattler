using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return Ok(new 
            {
                user.Id,
                user.Username,
                user.FullName,
                user.ProfilePictureUrl,
                user.Role,
                user.ExperiencePoints,
                user.Level,
                user.Credits,
                user.CosmeticItemsJson,
                user.SettingsJson,
                user.AchievementsJson,
                user.NeedsUsernameSetup,
                user.GoogleId,
                user.CreatedAt,
                user.LastLogin
            });
        }
    }
}
