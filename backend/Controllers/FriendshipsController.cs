using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/friendships")]
    public class FriendshipsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public FriendshipsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyFriends()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int myId))
                return Unauthorized();

            var friends = await _db.Friendships
                .Where(f => (f.RequesterId == myId || f.AddresseeId == myId) && f.IsConfirmed)
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .ToListAsync();

            var friendList = friends.Select(f =>
            {
                var user = f.RequesterId == myId ? f.Addressee : f.Requester;
                return new
                {
                    friendshipId = f.Id, 
                    id = user.Id,
                    username = user.Username,
                    fullName = user.FullName,
                    profilePictureUrl = user.ProfilePictureUrl,
                    lastLogin = user.LastLogin,
                    online = false
                };
            });

            return Ok(friendList);
        }

        [HttpGet("search-users")]
        [Authorize]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int myId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Ok(Array.Empty<object>());

            var users = await _db.Users
                .Where(u =>
                    u.Id != myId &&
                    (
                        EF.Functions.ILike(u.Username, $"%{query}%") ||
                        EF.Functions.ILike(u.FullName, $"%{query}%")
                    )
                )
                .OrderBy(u => u.Username)
                .Take(10)
                .ToListAsync();

            var myFriendships = await _db.Friendships
                .Where(f => (f.RequesterId == myId || f.AddresseeId == myId))
                .ToListAsync();

            var result = users.Select(u =>
            {
                var friendship = myFriendships.FirstOrDefault(f =>
                    (f.RequesterId == myId && f.AddresseeId == u.Id) ||
                    (f.RequesterId == u.Id && f.AddresseeId == myId)
                );

                string status = "none";
                if (friendship != null)
                {
                    if (friendship.IsConfirmed)
                        status = "friend";
                    else if (friendship.RequesterId == myId)
                        status = "pending_sent";
                    else
                        status = "pending_received";
                }

                return new
                {
                    id = u.Id,
                    username = u.Username,
                    fullName = u.FullName,
                    profilePictureUrl = u.ProfilePictureUrl,
                    status
                };
            });

            return Ok(result);
        }

        [HttpGet("requests")]
        [Authorize]
        public async Task<IActionResult> GetMyFriendRequests()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int myId))
                return Unauthorized();

            var requests = await _db.Friendships
                .Where(f => f.AddresseeId == myId && !f.IsConfirmed)
                .Include(f => f.Requester)
                .ToListAsync();

            var pendingRequests = requests.Select(f => new
            {
                id = f.Id,
                requesterId = f.Requester.Id,
                requesterUsername = f.Requester.Username,
                requesterProfilePictureUrl = f.Requester.ProfilePictureUrl,
                profilePictureUrl = f.Requester.ProfilePictureUrl,
                fullName = f.Requester.FullName, 
                createdAt = f.CreatedAt.ToUniversalTime().ToString("o")
            });

            return Ok(pendingRequests);
        }

        [HttpPost("accept/{friendshipId}")]
        [Authorize]
        public async Task<IActionResult> AcceptFriendRequest(int friendshipId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int myId))
                return Unauthorized();

            var friendship = await _db.Friendships.FirstOrDefaultAsync(f =>
                f.Id == friendshipId && f.AddresseeId == myId && !f.IsConfirmed);

            if (friendship == null)
                return NotFound(new { message = "Friend request not found." });

            friendship.IsConfirmed = true;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Friend request accepted." });
        }

        [HttpPost("request/{addresseeId}")]
        [Authorize]
        public async Task<IActionResult> SendFriendRequest(int addresseeId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int myId))
                return Unauthorized();

            if (myId == addresseeId)
                return BadRequest(new { message = "You cannot add yourself as a friend." });

            var exists = await _db.Friendships.AnyAsync(f =>
                (f.RequesterId == myId && f.AddresseeId == addresseeId) ||
                (f.RequesterId == addresseeId && f.AddresseeId == myId)
            );
            if (exists)
                return BadRequest(new { message = "Friendship or friend request already exists." });

            var friendship = new Friendship
            {
                RequesterId = myId,
                AddresseeId = addresseeId,
                IsConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Friendships.Add(friendship);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Friend request sent." });
        }

        [HttpDelete("{friendshipId}")]
        [Authorize]
        public async Task<IActionResult> DeleteFriendship(int friendshipId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int myId))
                return Unauthorized();

            var friendship = await _db.Friendships.FirstOrDefaultAsync(f =>
                f.Id == friendshipId &&
                (f.RequesterId == myId || f.AddresseeId == myId));

            if (friendship == null)
                return NotFound(new { message = "Friendship not found." });

            _db.Friendships.Remove(friendship);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Friendship/request removed." });
        }
    }
}
