public class BattleRequest
{
    public int PlayerId { get; set; }
    public int? AttackId { get; set; } 
    public int? EnemyHp { get; set; } 
    public string? EnemyName { get; set; }
    public string Action { get; set; } = string.Empty;
}
