using backend.Data;
using System;
using System.Collections.Generic;

namespace backend.Logic
{
    public class AttackResult
    {
        public int DamageToEnemy { get; set; }
        public int HealToPlayer { get; set; }
        public bool BlockNextAttack { get; set; }
        public bool EvadeNextAttack { get; set; }
        public bool ApplyPoison { get; set; }
        public string Log { get; set; } = "";
        public int CritChanceBonus { get; set; }
        public int CritBonusTurns { get; set; }

        // Poison DoT
        public int PoisonDamagePerTurn { get; set; }
        public int PoisonTurns { get; set; }
    }

    public static class AttackLogic
    {
        public static AttackResult ApplyAttack(
            AttackTemplate template,
            dynamic player,   // Name, Attack, Magic, Defense, Agility, Speed
            dynamic enemy     // Name, Type, Defense
        )
        {
            // Kör i double för att undvika tidig truncering
            double dmg = template.BaseDamage;
            double heal = template.HealAmount;

            bool block = template.BlockNextAttack;
            bool evade = template.EvadeNextAttack;
            bool poison = template.Poison;

            int critBonus = 0;
            int critTurns = 0;

            int pDmg = 0;
            int pTurns = 0;

            var effects = new List<string>();

            // === Scaling (double hela vägen) ===
            if (template.Scaling != null)
            {
                foreach (var pair in template.Scaling)
                {
                    var stat = pair.Key.ToLower();
                    double scale = pair.Value;

                    double statVal = stat switch
                    {
                        "attack"  => (double)(player.Attack  ?? 0),
                        "magic"   => (double)(player.Magic   ?? 0),
                        "defense" => (double)(player.Defense ?? 0),
                        "agility" => (double)(player.Agility ?? 0),
                        "speed"   => (double)(player.Speed   ?? 0),
                        _ => 0.0
                    };

                    dmg += scale * statVal;
                }
            }

            // === Specials / status-effekter ===
            switch (template.Name)
            {
                case "Holy Light":
                    // Healar alltid
                    effects.Add($"{player.Name} is healed for {(int)Math.Round(heal)} HP!");
                    // Extra skada vs undead
                    if ((enemy?.Type as string)?.ToLower() == "undead")
                    {
                        double magicScale = 0;
                        if (template.Scaling != null && template.Scaling.TryGetValue("magic", out var s))
                            magicScale = s;

                        double magicVal = (double)(player.Magic ?? 0);
                        dmg = template.BaseDamage + 10 + (magicScale * magicVal);

                        var enemyName = enemy?.Name ?? "the enemy";
                        effects.Add($"{player.Name} smites undead {enemyName}!");
                    }
                    else
                    {
                        // Ingen offensiv skada annars
                        dmg = 0;
                    }
                    break;

                case "Sacred Shield":
                case "Shield Block":
                case "Mana Shield":
                    block = true;
                    effects.Add($"{player.Name} prepares to block the next attack!");
                    // dessa har liten BaseDamage + ev. scaling redan i dmg
                    break;

                case "Poison Strike":
                case "Nature's Grasp":
                    poison = true;
                    pDmg = template.PoisonDamagePerTurn > 0 ? template.PoisonDamagePerTurn : 2;
                    pTurns = template.PoisonDuration > 0 ? template.PoisonDuration : 2;
                    effects.Add($"{player.Name} uses {template.Name} and applies poison!");
                    // dmg från base + scaling gäller också
                    break;

                case "Shadowstep":
                case "Camouflage":
                    evade = true;
                    critBonus = template.CritChanceBonus > 0 ? template.CritChanceBonus : 20;
                    critTurns = template.CritBonusTurns  > 0 ? template.CritBonusTurns  : 1;
                    effects.Add($"{player.Name} uses {template.Name} and becomes harder to hit!");
                    // Utility – ingen direkt skada
                    dmg = 0;
                    break;

                case "Battle Shout":
                    critBonus = template.CritChanceBonus > 0 ? template.CritChanceBonus : 20;
                    critTurns = template.CritBonusTurns  > 0 ? template.CritBonusTurns  : 1;
                    effects.Add($"{player.Name} uses Battle Shout and rallies for the next fights!");
                    // Buff – ingen direkt skada
                    dmg = 0;
                    break;

                default:
                    effects.Add($"{player.Name} uses {template.Name}!");
                    break;
            }

            // Sekundära effekter/loggar
            if (heal > 0 && template.Name != "Holy Light")
            {
                effects.Add($"{player.Name} heals for {(int)Math.Round(heal)} HP!");
            }
            if (block && !(template.Name == "Shield Block" || template.Name == "Sacred Shield" || template.Name == "Mana Shield"))
            {
                effects.Add($"{player.Name} prepares to block the next attack!");
            }
            if (evade)
            {
                effects.Add($"{player.Name} will evade the next attack!");
            }
            if (poison && template.Name != "Poison Strike")
            {
                var enemyName = enemy?.Name ?? "the enemy";
                effects.Add($"{player.Name} poisons {enemyName}!");
            }
            if (critBonus > 0 && critTurns > 0)
            {
                effects.Add($"{player.Name} gains +{critBonus}% crit chance for {critTurns} turn(s)!");
            }

            // === Slutrunda & mitigation ===
            int finalDamage = Math.Max(0, (int)Math.Round(dmg));
            int finalHeal   = Math.Max(0, (int)Math.Round(heal));

            // Dra av fiendens defense EFTER avrundning (som du önskade)
            int enemyDefense = 0;
            if (enemy != null)
            {
                try { enemyDefense = (int)enemy.Defense; } catch { enemyDefense = 0; }
            }
            finalDamage = Math.Max(0, finalDamage - enemyDefense);

            return new AttackResult
            {
                DamageToEnemy = finalDamage,
                HealToPlayer = finalHeal,
                BlockNextAttack = block,
                EvadeNextAttack = evade,
                ApplyPoison = poison,
                PoisonDamagePerTurn = poison ? pDmg : 0,
                PoisonTurns = poison ? pTurns : 0,

                CritChanceBonus = critBonus,
                CritBonusTurns = critTurns,
                Log = string.Join(" ", effects)
            };
        }
    }
}
