export interface Fighter {
  id: number; 
  name: string;
  hp: number;
  maxHp: number;
  // Lägg till fler stats om du vill visa t.ex. attack, defense, etc.
}

export interface BattleResponse {
  playerHp: number;
  playerMaxHp: number;
  playerEnergy: number;

  enemyHp: number;
  enemyMaxHp: number;
  enemyName: string;

  battleLog: BattleLogEntry[];
  battleEnded: boolean;
  gainedXp?: number;
  newExperiencePoints?: number;
  playerLevel?: number;
  userXp?: number;
  userLevel?: number;

  // Status flags från backend för effekter som visas i UI:
  isPlayerBlocking?: boolean;
  isEnemyPoisoned?: boolean;
  // Du kan lägga till fler: t.ex. isPlayerPoisoned, isEnemyBlocking, etc.
}

export interface BattleLogEntry {
  message: string;
  type: string; 
}

export interface PlayerAttack {
  id: number;
  name: string;
  type: string;
  damageType: string;
  baseDamage: number;
  maxCharges: number;
  currentCharges: number;
  scaling: { [stat: string]: number };
  requiredStats: { [stat: string]: number };
  allowedClasses: string[];
  description: string;
}
