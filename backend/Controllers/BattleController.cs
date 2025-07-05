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
                return BadRequest("Not enough energy to perform an attack.");

            // --- Enemy setup ---
            var enemy = new
            {
                Name = "Goblin",
                Hp = req.EnemyHp ?? 20,
                MaxHp = 20,
                Attack = 10,
                Defense = 1,
                xp = 40,
                CritChance = 0.05 // 5%
            };

            var rand = new Random();
            var log = new List<string>();

            if (req.Action == "attack")
            {
                // Battle intro log ONLY if this is the first turn (full HP on both)
                if ((enemy.Hp == enemy.MaxHp) && (player.CurrentHealth == player.MaxHealth))
                {
                    log.Add($"A wild {enemy.Name.ToLower()} appears! It looks dangerous...");
                    log.Add($"{player.Name} (Lv {player.Level}) cracks their knuckles and grins at the {enemy.Name.ToLower()}.");
                }

                // --- Player attack (funny random actions) ---
                string[] playerOpeners = new[]
                {
                    $"⚔️ {player.Name} charges forward and swings at the {enemy.Name.ToLower()}!",
                    $"{player.Name} does a cartwheel and throws a punch at the goblin!",
                    $"{player.Name} yells 'YEET!' and hurls themselves at the goblin.",
                    $"{player.Name} uses 100% of their remaining dignity to deliver an attack.",
                    $"{player.Name} slips on a banana peel but still manages to attack the goblin.",
                    $"{player.Name} whips out a rubber chicken and bonks the goblin.",
                    $"{player.Name} does a spinny anime sword move and aims at the goblin."
                };
                log.Add(playerOpeners[rand.Next(playerOpeners.Length)]);

                int basePlayerDamage = Math.Max(player.Attack - enemy.Defense, 1);
                double rngFactor = 0.8 + rand.NextDouble() * 0.4; // 0.8 to 1.2
                int rolledDamage = (int)Math.Round(basePlayerDamage * rngFactor);
                double critRoll = rand.NextDouble();
                bool isCrit = critRoll < player.CriticalChance;

                int playerDamage = rolledDamage;
                if (isCrit)
                {
                    playerDamage = (int)Math.Round((double)playerDamage * 2);
                    string[] critLines = new[]
                    {
                        $"💥 **Critical hit!** {player.Name} channels their inner anime protagonist and deals a jaw-dropping {playerDamage} damage!",
                        $"💥 **Critical hit!** {player.Name}'s attack goes SUPER SAIYAN and deals {playerDamage} damage!",
                        $"💥 **Critical hit!** {player.Name} lands an epic hit for {playerDamage} damage!",
                        $"💥 **Critical hit!** {player.Name} unleashes a forbidden meme attack for {playerDamage} damage!"
                    };
                    log.Add(critLines[rand.Next(critLines.Length)]);
                }
                // Friendly damage log (use exact phrase for frontend)
                log.Add($"{player.Name} deals {playerDamage} damage to the goblin.");

                string[] goblinHpLines = new[]
                {
                    $"The goblin staggers! Goblin HP now at {Math.Max(enemy.Hp - playerDamage, 0)}/{enemy.MaxHp}.",
                    $"Goblin now has {Math.Max(enemy.Hp - playerDamage, 0)}/{enemy.MaxHp} HP remaining.",
                    $"Goblin looks worried: {Math.Max(enemy.Hp - playerDamage, 0)}/{enemy.MaxHp} HP left."
                };
                log.Add(goblinHpLines[rand.Next(goblinHpLines.Length)]);

                int enemyHpNew = enemy.Hp - playerDamage;

                if (enemyHpNew <= 0)
                {
                    string[] victoryLines = new[]
                    {
                        $"🏆 **Victory!** The goblin explodes into confetti and is defeated!",
                        $"🏆 **Victory!** {player.Name} just flexed too hard. The goblin faints!",
                        $"🏆 **Victory!** The goblin can't handle the cringe and surrenders.",
                        $"🏆 **Victory!** The goblin collapses and is defeated!"
                    };
                    log.Add(victoryLines[rand.Next(victoryLines.Length)]);
                    log.Add($"{player.Name} gains {enemy.xp} XP from the battle.");
                    player.CurrentEnergy -= 1;

                    int gainedXp = enemy.xp;
                    player.ExperiencePoints += gainedXp;

                    // Level up
                    while (player.ExperiencePoints >= 100)
                    {
                        player.ExperiencePoints -= 100;
                        player.Level += 1;
                        log.Add($"🎉 {player.Name} has reached level {player.Level}!");

                        if (player.User != null)
                        {
                            player.User.ExperiencePoints += 10;
                            while (player.User.ExperiencePoints >= 100)
                            {
                                player.User.ExperiencePoints -= 100;
                                player.User.Level += 1;
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

                // --- Enemy attack (random and funny!) ---
                string[] goblinActions = new[]
                {
                    "😡 The goblin becomes enraged and counterattacks!",
                    "🤪 The goblin starts screaming random TikTok audios and attacks!",
                    "👺 The goblin insults your fashion sense before striking.",
                    "😈 The goblin attempts a triple backflip attack!",
                    "😤 The goblin roars 'I am the danger!' and lunges at you!",
                    "🤡 The goblin tells a bad dad joke and then attacks.",
                    "👹 The goblin throws a rock and hopes for the best."
                };
                log.Add(goblinActions[rand.Next(goblinActions.Length)]);

                string[] goblinAttackLines = new[]
                {
                    "The goblin raises its rusty sword high and swings it furiously!",
                    "The goblin lunges forward with a wild stab!",
                    "The goblin jumps and tries to bite you with sharp teeth!",
                    "The goblin throws a moldy shoe at your head!",
                    "The goblin attempts a WWE suplex, but just falls on you.",
                    "The goblin tries to trip you with its own nose hair!",
                    "The goblin slaps you with a slice of wet bread!"
                };
                log.Add(goblinAttackLines[rand.Next(goblinAttackLines.Length)]);

                int baseEnemyDamage = Math.Max(enemy.Attack - player.Defense, 1);
                double enemyRng = 0.8 + rand.NextDouble() * 0.4;
                int enemyRolledDamage = (int)Math.Round(baseEnemyDamage * enemyRng);
                double enemyCritRoll = rand.NextDouble();
                bool enemyCrit = enemyCritRoll < enemy.CritChance;

                int enemyDamage = enemyRolledDamage;
                if (enemyCrit)
                {
                    enemyDamage = (int)Math.Round((double)enemyDamage * 2);
                    string[] enemyCritLines = new[]
                    {
                        $"💥 **Critical hit!** The goblin spins like a fidget spinner and deals {enemyDamage} damage!",
                        $"💥 **Critical hit!** The goblin hits you with peak goblin rage for {enemyDamage} damage!",
                        $"💥 **Critical hit!** The goblin yells 'GOBLIN MODE' and deals {enemyDamage} damage!",
                        $"💥 **Critical hit!** The goblin goes berserk and hits you for {enemyDamage} damage!"
                    };
                    log.Add(enemyCritLines[rand.Next(enemyCritLines.Length)]);
                }

                // Enemy damage log (use exact phrase for frontend)
                log.Add($"{player.Name} takes {enemyDamage} damage from the attack.");

                string[] playerHpLines = new[]
                {
                    $"{player.Name} now has {Math.Max(player.CurrentHealth - enemyDamage, 0)}/{player.MaxHealth} HP remaining.",
                    $"{player.Name} stumbles but remains standing at {Math.Max(player.CurrentHealth - enemyDamage, 0)}/{player.MaxHealth} HP.",
                    $"{player.Name} grits their teeth, HP: {Math.Max(player.CurrentHealth - enemyDamage, 0)}/{player.MaxHealth}."
                };
                log.Add(playerHpLines[rand.Next(playerHpLines.Length)]);

                int playerHpNew = player.CurrentHealth - enemyDamage;
                player.CurrentHealth = Math.Max(0, playerHpNew);

                if (playerHpNew <= 0)
                {
                    string[] defeatLines = new[]
                    {
                        $"💀 {player.Name} slips on a banana peel and is KO'd by the goblin!",
                        $"💀 {player.Name} faints after seeing the goblin do a Fortnite dance.",
                        $"💀 {player.Name} falls to the ground, defeated by the goblin!",
                        $"💀 The goblin flexes, and {player.Name} just can't handle it."
                    };
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
                log.Add($"Goblin HP: {enemyHpNew}/{enemy.MaxHp}");

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
