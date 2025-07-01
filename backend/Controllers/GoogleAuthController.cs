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
                        PasswordHash = "",
                        Role = "User"
                    };

                    _db.Users.Add(user);
                    await _db.SaveChangesAsync();
                }

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
