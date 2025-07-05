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

        // Ny fiendetyp med egenskaper
        private class EnemyTemplate
        {
            public string Name { get; set; } = "";
            public int MaxHp { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int XP { get; set; }
            public double CritChance { get; set; }
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
            var enemyTemplates = new List<EnemyTemplate>
            {
                new EnemyTemplate { Name = "Goblin", MaxHp = 20, Attack = 10, Defense = 1, XP = 40, CritChance = 0.05 },
                new EnemyTemplate { Name = "Rat King", MaxHp = 24, Attack = 12, Defense = 2, XP = 60, CritChance = 0.08 },
                new EnemyTemplate { Name = "Angry Chicken", MaxHp = 16, Attack = 7, Defense = 0, XP = 25, CritChance = 0.12 },
                new EnemyTemplate { Name = "Skeleton Mage", MaxHp = 28, Attack = 15, Defense = 4, XP = 90, CritChance = 0.10 },
                new EnemyTemplate { Name = "Karen", MaxHp = 30, Attack = 14, Defense = 3, XP = 120, CritChance = 0.06 },
                new EnemyTemplate { Name = "Pixel Slime", MaxHp = 18, Attack = 8, Defense = 1, XP = 22, CritChance = 0.03 },
                new EnemyTemplate { Name = "Mime", MaxHp = 21, Attack = 9, Defense = 2, XP = 45, CritChance = 0.15 }
            };

            EnemyTemplate selectedEnemy;
            int enemyHp;
            if (req.EnemyHp == null || req.EnemyHp == 0)
            {
                selectedEnemy = enemyTemplates[rand.Next(enemyTemplates.Count)];
                enemyHp = selectedEnemy.MaxHp;
            }
            else
            {
                var nameFromFrontend = req.EnemyName ?? "Goblin";
                selectedEnemy = enemyTemplates.FirstOrDefault(e => e.Name == nameFromFrontend) ?? enemyTemplates[0];
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
                    log.Add($"A wild {enemy.Name.ToLower()} appears! It looks dangerous...");
                    log.Add($"{player.Name} (Lv {player.Level}) cracks their knuckles and grins at the {enemy.Name.ToLower()}.");
                }
                string[] playerOpeners = new[]
                {
                    $"⚔️ {player.Name} charges forward and swings at the {enemy.Name.ToLower()}!",
                    $"{player.Name} does a cartwheel and throws a punch at the {enemy.Name.ToLower()}!",
                    $"{player.Name} yells 'YEET!' and hurls themselves at the {enemy.Name.ToLower()}.",
                    $"{player.Name} uses 100% of their remaining dignity to deliver an attack.",
                    $"{player.Name} slips on a banana peel but still manages to attack the {enemy.Name.ToLower()}.",
                    $"{player.Name} whips out a rubber chicken and bonks the {enemy.Name.ToLower()}.",
                    $"{player.Name} does a spinny anime sword move and aims at the {enemy.Name.ToLower()}."
                };
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
                    string[] critLines = new[]
                    {
                        $"💥 **Critical hit!** {player.Name} channels their inner anime protagonist and deals a jaw-dropping {playerDamage} damage!",
                        $"💥 **Critical hit!** {player.Name}'s attack goes SUPER SAIYAN and deals {playerDamage} damage!",
                        $"💥 **Critical hit!** {player.Name} lands an epic hit for {playerDamage} damage!",
                        $"💥 **Critical hit!** {player.Name} unleashes a forbidden meme attack for {playerDamage} damage!"
                    };
                    log.Add(critLines[rand.Next(critLines.Length)]);
                }
                log.Add($"{player.Name} deals {playerDamage} damage to the {enemy.Name.ToLower()}.");

                string[] enemyHpLines = new[]
                {
                    $"The {enemy.Name.ToLower()} staggers! {enemy.Name} HP now at {Math.Max(enemy.Hp - playerDamage, 0)}/{enemy.MaxHp}.",
                    $"{enemy.Name} now has {Math.Max(enemy.Hp - playerDamage, 0)}/{enemy.MaxHp} HP remaining.",
                    $"{enemy.Name} looks worried: {Math.Max(enemy.Hp - playerDamage, 0)}/{enemy.MaxHp} HP left."
                };
                log.Add(enemyHpLines[rand.Next(enemyHpLines.Length)]);

                int enemyHpNew = enemy.Hp - playerDamage;

                if (enemyHpNew <= 0)
                {
                    string[] victoryLines = new[]
                    {
                        $"🏆 **Victory!** The {enemy.Name.ToLower()} explodes into confetti and is defeated!",
                        $"🏆 **Victory!** {player.Name} just flexed too hard. The {enemy.Name.ToLower()} faints!",
                        $"🏆 **Victory!** The {enemy.Name.ToLower()} can't handle the cringe and surrenders.",
                        $"🏆 **Victory!** The {enemy.Name.ToLower()} collapses and is defeated!",
                        $"🏆 **Victory!** {player.Name} does a victory dance while the {enemy.Name.ToLower()} lies defeated."
                    };
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

                string[] enemyActions = new[]
                {
                    $"😡 The {enemy.Name.ToLower()} becomes enraged and counterattacks!",
                    $"🤪 The {enemy.Name.ToLower()} starts screaming random TikTok audios and attacks!",
                    $"👺 The {enemy.Name.ToLower()} insults your fashion sense before striking.",
                    $"😈 The {enemy.Name.ToLower()} attempts a triple backflip attack!",
                    $"😤 The {enemy.Name.ToLower()} roars 'I am the danger!' and lunges at you!",
                    $"🤡 The {enemy.Name.ToLower()} tells a bad dad joke and then attacks.",
                    $"👹 The {enemy.Name.ToLower()} throws a rock and hopes for the best.",
                    $"🦖 The {enemy.Name.ToLower()} tries to do a dinosaur roar but just ends up coughing.",
                    $"🧙‍♂️ The {enemy.Name.ToLower()} casts a spell that backfires and hits itself!",
                    $"👻 The {enemy.Name.ToLower()} tries to scare you but just ends up falling over."
                };
                log.Add(enemyActions[rand.Next(enemyActions.Length)]);

                string[] enemyAttackLines = new[]
                {
                    $"The {enemy.Name.ToLower()} raises its rusty sword high and swings it furiously!",
                    $"The {enemy.Name.ToLower()} lunges forward with a wild stab!",
                    $"The {enemy.Name.ToLower()} jumps and tries to bite you with sharp teeth!",
                    $"The {enemy.Name.ToLower()} throws a moldy shoe at your head!",
                    $"The {enemy.Name.ToLower()} attempts a WWE suplex, but just falls on you.",
                    $"The {enemy.Name.ToLower()} tries to trip you with its own nose hair!",
                    $"The {enemy.Name.ToLower()} slaps you with a slice of wet bread!",
                    $"The {enemy.Name.ToLower()} does a dramatic spin and accidentally hits itself!"
                };
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
                    string[] enemyCritLines = new[]
                    {
                        $"💥 **Critical hit!** The {enemy.Name.ToLower()} spins like a fidget spinner and deals {enemyDamage} damage!",
                        $"💥 **Critical hit!** The {enemy.Name.ToLower()} hits you with peak rage for {enemyDamage} damage!",
                        $"💥 **Critical hit!** The {enemy.Name.ToLower()} yells 'GO {enemy.Name.ToUpper()} MODE' and deals {enemyDamage} damage!",
                        $"💥 **Critical hit!** The {enemy.Name.ToLower()} goes berserk and hits you for {enemyDamage} damage!"
                    };
                    log.Add(enemyCritLines[rand.Next(enemyCritLines.Length)]);
                }

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
                        $"💀 {player.Name} slips on a banana peel and is KO'd by the {enemy.Name.ToLower()}!",
                        $"💀 {player.Name} faints after seeing the {enemy.Name.ToLower()} do a Fortnite dance.",
                        $"💀 {player.Name} falls to the ground, defeated by the {enemy.Name.ToLower()}!",
                        $"💀 The {enemy.Name.ToLower()} flexes, and {player.Name} just can't handle it."
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

            var enemyTemplates = new List<EnemyTemplate>
            {
                new EnemyTemplate { Name = "Goblin", MaxHp = 20, Attack = 10, Defense = 1, XP = 40, CritChance = 0.05 },
                new EnemyTemplate { Name = "Rat King", MaxHp = 24, Attack = 12, Defense = 2, XP = 60, CritChance = 0.08 },
                new EnemyTemplate { Name = "Angry Chicken", MaxHp = 16, Attack = 7, Defense = 0, XP = 25, CritChance = 0.12 },
                new EnemyTemplate { Name = "Skeleton Mage", MaxHp = 28, Attack = 15, Defense = 4, XP = 90, CritChance = 0.10 },
                new EnemyTemplate { Name = "Karen", MaxHp = 30, Attack = 14, Defense = 3, XP = 120, CritChance = 0.06 },
                new EnemyTemplate { Name = "Pixel Slime", MaxHp = 18, Attack = 8, Defense = 1, XP = 22, CritChance = 0.03 },
                new EnemyTemplate { Name = "Mime", MaxHp = 21, Attack = 9, Defense = 2, XP = 45, CritChance = 0.15 }
            };

            var selectedEnemy = enemyTemplates[rand.Next(enemyTemplates.Count)];

            return Ok(new {
                enemyName = selectedEnemy.Name,
                enemyHp = selectedEnemy.MaxHp,
                enemyMaxHp = selectedEnemy.MaxHp,
            });
        }
    }
}
