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

        // GET: api/users/me
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

        // GET: api/users/{username}
        [HttpGet("{username}")]
        [Authorize]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { message = "Username is required" });

            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

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
                user.CreatedAt,
                user.LastLogin
            });
        }

        // GET: api/users
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _db.Users
                .Select(user => new
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
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
