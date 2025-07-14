namespace backend.Models
{
    public class BattleState
    {
        // Spelarens tillstånd
        public bool PlayerIsBlocking { get; set; } = false;
        public int PlayerBlockTurnsLeft { get; set; } = 0;

        // Fiendens tillstånd
        public bool EnemyIsPoisoned { get; set; } = false;
        public int EnemyPoisonTurnsLeft { get; set; } = 0;
        public int EnemyPoisonDamage { get; set; } = 0;

        // Annat tillstånd vid behov
        public bool PlayerIsStealthed { get; set; } = false;
        public int PlayerStealthTurnsLeft { get; set; } = 0;

        // Lägg till fler effekter här om du behöver!
    }
}
