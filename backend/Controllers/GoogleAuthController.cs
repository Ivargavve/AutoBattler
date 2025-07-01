using backend.Data;
using backend.Models;
using backend.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/googleauth")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public GoogleAuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] CredentialDto request)
        {
            try
            {
                // Validate Google token payload
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential);

                // Find user by GoogleId (subject) to avoid username conflicts
                var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject);

                if (user == null)
                {
                    // Create new user with empty username; user must set username later
                    user = new User
                    {
                        Username = "", // Empty username initially
                        Role = "User",
                        FullName = payload.Name ?? "",
                        ProfilePictureUrl = payload.Picture ?? "",
                        GoogleId = payload.Subject ?? "",
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = DateTime.UtcNow,
                        ExperiencePoints = 0,
                        Level = 1,
                        Credits = 0,
                        CosmeticItemsJson = "{}",
                        SettingsJson = "{}",
                        AchievementsJson = "{}",
                        NeedsUsernameSetup = true // Flag to require username setup on first login
                    };

                    _db.Users.Add(user);
                }
                else
                {
                    // Update user information on each login (optional)
                    user.FullName = payload.Name ?? user.FullName;
                    user.ProfilePictureUrl = payload.Picture ?? user.ProfilePictureUrl;
                    user.LastLogin = DateTime.UtcNow;

                    _db.Users.Update(user);
                }

                await _db.SaveChangesAsync();

                // Create JWT token for authenticated user
                var token = _jwt.CreateToken(user);

                // Return token and user info, including flag if username setup is needed
                return Ok(new 
                { 
                    token,
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
                    user.NeedsUsernameSetup
                });
            }
            catch (InvalidJwtException)
            {
                return BadRequest("Invalid Google token");
            }
        }

        [HttpPut("set-username")]
        public async Task<IActionResult> SetUsername(SetUsernameDto request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null) return NotFound("User not found");

            // Check if the new username is already taken by another user
            var exists = await _db.Users.AnyAsync(u => u.Username == request.NewUsername && u.Id != request.UserId);
            if (exists) return BadRequest("Username already taken");

            // Set the new username and mark that setup is complete
            user.Username = request.NewUsername;
            user.NeedsUsernameSetup = false;

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Username updated successfully" });
        }

        // DTO to receive Google token credential from frontend
        public record CredentialDto(string Credential);

        // DTO for setting username request
        public record SetUsernameDto(int UserId, string NewUsername);
    }
}
