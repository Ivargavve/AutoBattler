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
    }

    public static class AttackLogic
    {
        public static AttackResult ApplyAttack(
            AttackTemplate template,
            dynamic player,   // förväntas ha Name, Attack, Magic, Defense, Agility, Speed
            dynamic enemy     // förväntas ha Name, Type
        )
        {
            int damage = template.BaseDamage;
            int heal = template.HealAmount;
            bool block = template.BlockNextAttack;
            bool evade = template.EvadeNextAttack;
            bool poison = template.Poison;

            int critBonus = 0;
            int critTurns = 0;

            var effects = new List<string>();

            // Scaling
            if (template.Scaling != null)
            {
                foreach (var pair in template.Scaling)
                {
                    var stat = pair.Key.ToLower();
                    double scale = pair.Value;
                    switch (stat)
                    {
                        case "attack":  damage += (int)(scale * (player.Attack  ?? 0)); break;
                        case "magic":   damage += (int)(scale * (player.Magic   ?? 0)); break;
                        case "defense": damage += (int)(scale * (player.Defense ?? 0)); break;
                        case "agility": damage += (int)(scale * (player.Agility ?? 0)); break;
                        case "speed":   damage += (int)(scale * (player.Speed   ?? 0)); break;
                    }
                }
            }

            switch (template.Name)
            {
                case "Holy Light":
                    effects.Add($"{player.Name} is healed for {heal} HP!");
                    if ((enemy?.Type as string)?.ToLower() == "undead")
                    {
                        double magicScale = (template.Scaling != null) ? template.Scaling.GetValueOrDefault("magic", 0) : 0;
                        damage = template.BaseDamage + 10 + (int)(magicScale * (player.Magic ?? 0));
                        var enemyName = enemy?.Name ?? "the enemy";
                        effects.Add($"{player.Name} smites undead {enemyName}!");
                    }
                    else
                    {
                        damage = 0;
                    }
                    break;

                case "Sacred Shield":
                case "Shield Block":
                case "Mana Shield":
                    block = true;
                    effects.Add($"{player.Name} prepares to block the next attack!");
                    break;

                case "Poison Strike":
                    poison = true;
                    effects.Add($"{player.Name} uses {template.Name} and poisons {enemy.Name}!");
                    break;

                case "Shadowstep":
                case "Camouflage":
                    evade = true;
                    critBonus = template.CritChanceBonus > 0 ? template.CritChanceBonus : 20;
                    critTurns = template.CritBonusTurns  > 0 ? template.CritBonusTurns  : 1;
                    effects.Add($"{player.Name} uses {template.Name} and becomes harder to hit!");
                    damage = 0;
                    break;

                case "Battle Shout":
                    effects.Add($"{player.Name} uses Battle Shout and increases attack for 2 turns!");
                    damage = 0;
                    break;

                case "Nature's Grasp":
                    effects.Add($"{player.Name} roots the enemy with nature's grasp!");
                    break;

                default:
                    effects.Add($"{player.Name} uses {template.Name}!");
                    break;
            }

            // Generell följdlogik
            if (heal > 0 && template.Name != "Holy Light")
            {
                effects.Add($"{player.Name} heals for {heal} HP!");
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

            return new AttackResult
            {
                DamageToEnemy = Math.Max(0, damage),
                HealToPlayer = Math.Max(0, heal),
                BlockNextAttack = block,
                EvadeNextAttack = evade,
                ApplyPoison = poison,
                CritChanceBonus = critBonus,
                CritBonusTurns = critTurns,
                Log = string.Join(" ", effects)
            };
        }
    }
}
