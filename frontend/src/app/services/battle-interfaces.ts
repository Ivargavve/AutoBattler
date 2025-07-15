export interface Fighter {
  id: number; 
  name: string;
  hp: number;
  maxHp: number;
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

  isPlayerBlocking?: boolean;
  isEnemyPoisoned?: boolean;
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
