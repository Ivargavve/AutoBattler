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
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential);

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        Username = payload.Email,
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
                        AchievementsJson = "{}"
                    };

                    _db.Users.Add(user);
                }
                else
                {
                    // Update user info on login (optional)
                    user.FullName = payload.Name ?? user.FullName;
                    user.ProfilePictureUrl = payload.Picture ?? user.ProfilePictureUrl;
                    user.GoogleId = payload.Subject ?? user.GoogleId;
                    user.LastLogin = DateTime.UtcNow;

                    _db.Users.Update(user);
                }

                await _db.SaveChangesAsync();

                var token = _jwt.CreateToken(user);
                return Ok(new { token });
            }
            catch (InvalidJwtException)
            {
                return BadRequest("Invalid Google token");
            }
        }

        public record CredentialDto(string Credential);
    }
}
