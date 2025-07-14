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
        public bool ApplyPoison { get; set; }
        public string Log { get; set; } = "";
    }

    public static class AttackLogic
    {
        public static AttackResult ApplyAttack(
            AttackTemplate template,
            dynamic player,  // Du kan använda din Character här (har nu rätt stats!)
            dynamic enemy    // Kan vara EnemyTemplate eller ett anonymt objekt
        )
        {
            int damage = template.BaseDamage;
            int heal = template.HealAmount;
            bool block = template.BlockNextAttack;
            bool poison = template.Poison;
            string log = "";
            var effects = new List<string>();

            // --- SCALING ---
            if (template.Scaling != null)
            {
                foreach (var pair in template.Scaling)
                {
                    var stat = pair.Key.ToLower();
                    double scale = pair.Value;
                    switch (stat)
                    {
                        case "attack": damage += (int)(scale * (player.Attack ?? 0)); break;
                        case "magic": damage += (int)(scale * (player.Magic ?? 0)); break;
                        case "defense": damage += (int)(scale * (player.Defense ?? 0)); break;
                        case "agility": damage += (int)(scale * (player.Agility ?? 0)); break;
                        case "speed": damage += (int)(scale * (player.Speed ?? 0)); break;
                        // Lägg till fler stats här om du har fler i din modell!
                    }
                }
            }

            // --- SPECIALFALL FÖR NAMNGIVNA ATTACKER ---
            switch (template.Name)
            {
                case "Holy Light":
                    effects.Add($"{player.Name} is healed for {heal} HP!");
                    if ((enemy?.Type as string)?.ToLower() == "undead")
                    {
                        double magicScale = (template.Scaling != null) ? template.Scaling.GetValueOrDefault("magic", 0) : 0;
                        damage = template.BaseDamage + (int)(magicScale * (player.Magic ?? 0));
                        var enemyName = enemy?.Name ?? "the enemy";
                        effects.Add($"{player.Name} smites undead {enemyName} for {damage} holy damage!");
                    }
                    else
                    {
                        damage = 0; // Ingen skada mot icke-undead
                    }
                    break;

                case "Sacred Shield":
                case "Shield Block":
                case "Mana Shield":
                    block = true;
                    if (damage > 0)
                        effects.Add($"{player.Name} blocks the next attack and hits for {damage} damage!");
                    else
                        effects.Add($"{player.Name} prepares to block the next attack!");
                    break;

                case "Poison Strike":
                    poison = true;
                    effects.Add($"{player.Name} uses {template.Name} and poisons {enemy.Name} for {damage} damage!");
                    break;

                case "Shadowstep":
                case "Camouflage":
                    effects.Add($"{player.Name} uses {template.Name} and becomes harder to hit!");
                    damage = 0;
                    break;

                case "Battle Shout":
                    // Här kan du lägga till buff-return, just nu bara log
                    effects.Add($"{player.Name} uses Battle Shout and increases attack for 2 turns!");
                    damage = 0;
                    break;

                case "Nature's Grasp":
                    effects.Add($"{player.Name} roots the enemy with nature's grasp and deals {damage} nature damage!");
                    break;

                default:
                    if (damage > 0)
                        effects.Add($"{player.Name} uses {template.Name} and deals {damage} damage.");
                    else
                        effects.Add($"{player.Name} uses {template.Name}!");
                    break;
            }

            // --- GENERELLA EFFEKTER OM INTE REDAN LOGGADE ---
            if (heal > 0 && template.Name != "Holy Light")
            {
                effects.Add($"{player.Name} heals for {heal} HP!");
            }
            if (block && !(template.Name == "Shield Block" || template.Name == "Sacred Shield" || template.Name == "Mana Shield"))
            {
                effects.Add($"{player.Name} prepares to block the next attack!");
            }
            if (poison && template.Name != "Poison Strike")
            {
                var enemyName = enemy?.Name ?? "the enemy";
                effects.Add($"{player.Name} poisons {enemyName}!");
            }

            log = string.Join(" ", effects);

            return new AttackResult
            {
                DamageToEnemy = Math.Max(0, damage),
                HealToPlayer = Math.Max(0, heal),
                BlockNextAttack = block,
                ApplyPoison = poison,
                Log = log
            };
        }
    }
}
