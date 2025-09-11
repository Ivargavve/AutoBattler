using System.Collections.Generic;

namespace backend.Models
{
    public class PlayerAttack
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string DamageType { get; set; } = "";
        public int BaseDamage { get; set; }
        public int MaxCharges { get; set; }
        public int CurrentCharges { get; set; }
        public Dictionary<string, double>? Scaling { get; set; }
        public Dictionary<string, int>? RequiredStats { get; set; }
        public List<string>? AllowedClasses { get; set; }
        public string Description { get; set; } = "";
    }
}
