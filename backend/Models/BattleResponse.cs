public class BattleResponse
{
    public int PlayerHp { get; set; }
    public int PlayerMaxHp { get; set; }
    public int PlayerEnergy { get; set; }          
    public int EnemyHp { get; set; }
    public string EnemyName { get; set; } = string.Empty;
    public int EnemyMaxHp { get; set; }
    public List<string> BattleLog { get; set; } = new List<string>();
    public bool BattleEnded { get; set; }
    public int GainedXp { get; set; }
    public int NewExperiencePoints { get; set; }
    public int PlayerLevel { get; set; }
    public int UserXp { get; set; }
    public int UserLevel { get; set; }
    
    
    public bool IsBlocking { get; set; }       
    public bool EnemyIsPoisoned { get; set; }  
    public int HealThisTurn { get; set; }      
    public int PoisonDamageThisTurn { get; set; } 
}
