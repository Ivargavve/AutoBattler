namespace backend.Models
{
    public class BattleState
    {
        public bool PlayerIsBlocking { get; set; } = false;
        public int PlayerBlockTurnsLeft { get; set; } = 0;

        public bool EnemyIsPoisoned { get; set; } = false;
        public int EnemyPoisonTurnsLeft { get; set; } = 0;
        public int EnemyPoisonDamage { get; set; } = 0;

        public bool PlayerIsStealthed { get; set; } = false;
        public int PlayerStealthTurnsLeft { get; set; } = 0;
    }
}
