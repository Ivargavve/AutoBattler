using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using backend.Utils;

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
                return BadRequest("Not enough energy to perform an attack.");

            var rand = new Random();
            var enemyTemplates = EnemyTemplates.All;

            EnemyTemplate selectedEnemy;
            int enemyHp;

            if (req.EnemyHp == null || req.EnemyHp == 0)
            {
                selectedEnemy = enemyTemplates[rand.Next(enemyTemplates.Count)];
                enemyHp = selectedEnemy.MaxHp;
            }
            else
            {
                var nameFromFrontend = req.EnemyName ?? "Enemy";
                selectedEnemy = EnemyTemplates.GetByName(nameFromFrontend) ?? enemyTemplates[0];
                enemyHp = req.EnemyHp ?? selectedEnemy.MaxHp;
            }

            var enemy = new
            {
                Name = selectedEnemy.Name,
                Hp = enemyHp,
                MaxHp = selectedEnemy.MaxHp,
                Attack = selectedEnemy.Attack,
                Defense = selectedEnemy.Defense,
                xp = selectedEnemy.XP,
                CritChance = selectedEnemy.CritChance
            };

            var log = new List<string>();
            if (req.Action == "attack")
            {
                if ((enemy.Hp == enemy.MaxHp) && (player.CurrentHealth == player.MaxHealth))
                {
                    var encounterLines = BattleDialogLines.EncounterOpeners(player.Name, player.Level, enemy.Name.ToLower());
                    foreach (var line in encounterLines) log.Add(line);
                }
                var playerOpeners = BattleDialogLines.PlayerOpeners(player.Name, enemy.Name.ToLower());
                log.Add(playerOpeners[rand.Next(playerOpeners.Length)]);

                int basePlayerDamage = Math.Max(player.Attack - enemy.Defense, 1);
                double rngFactor = 0.8 + rand.NextDouble() * 0.4;
                int rolledDamage = (int)Math.Round(basePlayerDamage * rngFactor);
                double critRoll = rand.NextDouble();
                bool isCrit = critRoll < player.CriticalChance;

                int playerDamage = rolledDamage;
                if (isCrit)
                {
                    playerDamage = (int)Math.Round((double)playerDamage * 2);
                    var critLines = BattleDialogLines.CritLines(player.Name, playerDamage);
                    log.Add(critLines[rand.Next(critLines.Length)]);
                }
                log.Add($"{player.Name} deals {playerDamage} damage to the {enemy.Name.ToLower()}.");

                int enemyHpNew = enemy.Hp - playerDamage;
                var enemyHpLines = BattleDialogLines.EnemyHpLines(enemy.Name, Math.Max(enemyHpNew, 0), enemy.MaxHp);
                log.Add(enemyHpLines[rand.Next(enemyHpLines.Length)]);

                if (enemyHpNew <= 0)
                {
                    var victoryLines = BattleDialogLines.VictoryLines(player.Name, enemy.Name.ToLower());
                    log.Add(victoryLines[rand.Next(victoryLines.Length)]);
                    log.Add($"{player.Name} gains {enemy.xp} XP from the battle.");
                    player.CurrentEnergy -= 1;

                    int gainedXp = enemy.xp;
                    player.ExperiencePoints += gainedXp;

                    if (player.ExperiencePoints >= 100)
                    {
                        player.ExperiencePoints = 0;
                        player.Level += 1;
                        player.MaxHealth += 5;
                        player.CurrentHealth = player.MaxHealth;
                        player.Attack += 2;
                        player.Defense += 1;
                        player.CriticalChance += 0.01;
                        log.Add($"🎉 {player.Name} has reached level {player.Level}!");

                        if (player.User != null)
                        {
                            player.User.ExperiencePoints += 10;
                            if (player.User.ExperiencePoints >= 100)
                            {
                                player.User.ExperiencePoints = 0;
                                player.User.Level += 1;
                                player.User.Credits += 100;
                                log.Add($"🎉 Your account has reached user level {player.User.Level}!");
                            }
                        }
                    }
                    await _db.SaveChangesAsync();

                    log.Add($"Status: {player.Name} is now Level {player.Level}, XP: {player.ExperiencePoints}/100, Energy: {player.CurrentEnergy}/{player.MaxEnergy}");

                    return Ok(new BattleResponse
                    {
                        PlayerHp = player.CurrentHealth,
                        PlayerMaxHp = player.MaxHealth,
                        EnemyHp = 0,
                        EnemyMaxHp = enemy.MaxHp,
                        EnemyName = enemy.Name,
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

                var enemyActions = BattleDialogLines.EnemyActions(enemy.Name.ToLower());
                log.Add(enemyActions[rand.Next(enemyActions.Length)]);

                var enemyAttackLines = BattleDialogLines.EnemyAttackLines(enemy.Name.ToLower());
                log.Add(enemyAttackLines[rand.Next(enemyAttackLines.Length)]);

                int baseEnemyDamage = Math.Max(enemy.Attack - player.Defense, 1);
                double enemyRng = 0.8 + rand.NextDouble() * 0.4;
                int enemyRolledDamage = (int)Math.Round(baseEnemyDamage * enemyRng);
                double enemyCritRoll = rand.NextDouble();
                bool enemyCrit = enemyCritRoll < enemy.CritChance;

                int enemyDamage = enemyRolledDamage;
                if (enemyCrit)
                {
                    enemyDamage = (int)Math.Round((double)enemyDamage * 2);
                    var enemyCritLines = BattleDialogLines.EnemyCritLines(enemy.Name.ToLower(), enemyDamage);
                    log.Add(enemyCritLines[rand.Next(enemyCritLines.Length)]);
                }

                log.Add($"{player.Name} takes {enemyDamage} damage from the attack.");

                int playerHpNew = player.CurrentHealth - enemyDamage;
                var playerHpLines = BattleDialogLines.PlayerHpLines(player.Name, Math.Max(playerHpNew, 0), player.MaxHealth);
                log.Add(playerHpLines[rand.Next(playerHpLines.Length)]);

                player.CurrentHealth = Math.Max(0, playerHpNew);

                if (playerHpNew <= 0)
                {
                    var defeatLines = BattleDialogLines.DefeatLines(player.Name, enemy.Name.ToLower());
                    log.Add(defeatLines[rand.Next(defeatLines.Length)]);
                    player.CurrentHealth = 0;
                    player.CurrentEnergy = 0;
                    player.ExperiencePoints = 0;
                    player.Level = 1;
                    await _db.SaveChangesAsync();

                    log.Add($"Status: {player.Name} has reset to Level {player.Level}, XP: {player.ExperiencePoints}, Energy: {player.CurrentEnergy}/{player.MaxEnergy}");

                    return Ok(new BattleResponse
                    {
                        PlayerHp = 0,
                        PlayerMaxHp = player.MaxHealth,
                        EnemyHp = enemyHpNew,
                        EnemyMaxHp = enemy.MaxHp,
                        EnemyName = enemy.Name,
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

                log.Add($"--- End of turn ---");
                log.Add($"{player.Name} HP: {player.CurrentHealth}/{player.MaxHealth} | Energy: {player.CurrentEnergy}/{player.MaxEnergy}");
                log.Add($"{enemy.Name} HP: {enemyHpNew}/{enemy.MaxHp}");

                return Ok(new BattleResponse
                {
                    PlayerHp = playerHpNew,
                    PlayerMaxHp = player.MaxHealth,
                    EnemyHp = enemyHpNew,
                    EnemyMaxHp = enemy.MaxHp,
                    EnemyName = enemy.Name,
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

        [HttpGet("encounter")]
        public IActionResult EncounterEnemy()
        {
            var rand = new Random();

            var enemyTemplates = EnemyTemplates.All;
            var selectedEnemy = enemyTemplates[rand.Next(enemyTemplates.Count)];

            return Ok(new {
                enemyName = selectedEnemy.Name,
                enemyHp = selectedEnemy.MaxHp,
                enemyMaxHp = selectedEnemy.MaxHp,
            });
        }
    }
}
