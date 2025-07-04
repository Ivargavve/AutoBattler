using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BattleController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BattleController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("turn")]
        public async Task<IActionResult> BattleTurn([FromBody] BattleRequest req)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var player = await _db.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (player == null)
                return NotFound();

            if (player.CurrentEnergy <= 0)
            {
                return BadRequest("Not enough energy to perform an attack.");
            }

            // goblin
            var enemy = new
            {
                Name = "Goblin",
                Hp = req.EnemyHp ?? 20,
                MaxHp = 20,
                Attack = 10,
                Defense = 1,
                xp = 40
            };

            var log = new List<string>();
            

            if (req.Action == "attack")
            {
                int playerDamage = Math.Max(player.Attack - enemy.Defense, 1);
                int enemyHpNew = enemy.Hp - playerDamage;
                log.Add($"{player.Name} attacks {enemy.Name} for {playerDamage} damage!");

                if (enemyHpNew <= 0)
                {
                    log.Add($"Victory! {enemy.Name} is defeated!");
                    log.Add($"{player.Name} gains {enemy.xp} XP!");
                    player.CurrentEnergy -= 1;

                    int gainedXp = enemy.xp;
                    player.ExperiencePoints += gainedXp;
                    while (player.ExperiencePoints >= 100)
                    {
                        player.ExperiencePoints -= 100;
                        player.Level += 1;
                        log.Add($"{player.Name} leveled up to level {player.Level}!");

                        if (player.User != null)
                        {
                            player.User.ExperiencePoints += 10;

                            while (player.User.ExperiencePoints >= 100)
                            {
                                player.User.ExperiencePoints -= 100;
                                player.User.Level += 1;
                                log.Add($"User leveled up to level {player.User.Level}!");
                            }
                        }
                    }
                    await _db.SaveChangesAsync();

                    return Ok(new BattleResponse
                    {
                        PlayerHp = player.CurrentHealth,
                        PlayerMaxHp = player.MaxHealth,
                        EnemyHp = 0,
                        EnemyMaxHp = enemy.MaxHp,
                        BattleLog = log,
                        BattleEnded = true,
                        GainedXp = gainedXp,
                        NewExperiencePoints = player.ExperiencePoints,
                        PlayerLevel = player.Level,
                        UserXp = player.User?.ExperiencePoints ?? 0,
                        UserLevel = player.User?.Level ?? 0,
                        PlayerEnergy = player.CurrentEnergy
                    });
                }

                int enemyDamage = Math.Max(enemy.Attack - player.Defense, 1);
                int playerHpNew = player.CurrentHealth - enemyDamage;
                log.Add($"{enemy.Name} attacks {player.Name} for {enemyDamage} damage!");

                player.CurrentHealth = Math.Max(0, playerHpNew);

                if (playerHpNew <= 0)
                {
                    log.Add($"{player.Name} is defeated!");
                    player.CurrentHealth = 0;
                    player.CurrentEnergy = 0;
                    player.ExperiencePoints = 0;
                    player.Level = 1;
                    await _db.SaveChangesAsync();

                    return Ok(new BattleResponse
                    {
                        PlayerHp = 0,
                        PlayerMaxHp = player.MaxHealth,
                        EnemyHp = enemyHpNew,
                        EnemyMaxHp = enemy.MaxHp,
                        BattleLog = log,
                        BattleEnded = true,
                        GainedXp = 0,
                        NewExperiencePoints = player.ExperiencePoints,
                        PlayerLevel = player.Level,
                        UserXp = player.User?.ExperiencePoints ?? 0,
                        UserLevel = player.User?.Level ?? 0,
                        PlayerEnergy = player.CurrentEnergy
                    });
                }

                await _db.SaveChangesAsync();

                return Ok(new BattleResponse
                {
                    PlayerHp = playerHpNew,
                    PlayerMaxHp = player.MaxHealth,
                    EnemyHp = enemyHpNew,
                    EnemyMaxHp = enemy.MaxHp,
                    BattleLog = log,
                    BattleEnded = false,
                    GainedXp = 0,
                    NewExperiencePoints = player.ExperiencePoints,
                    PlayerLevel = player.Level,
                    UserXp = player.User?.ExperiencePoints ?? 0,
                    UserLevel = player.User?.Level ?? 0,
                    PlayerEnergy = player.CurrentEnergy
                });
            }

            return BadRequest("Unknown action");
        }
    }
}
