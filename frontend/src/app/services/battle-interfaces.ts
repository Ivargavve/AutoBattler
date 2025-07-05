export interface Fighter {
  name: string;
  hp: number;
  maxHp: number;
}

export interface BattleResponse {
  playerHp: number;
  playerMaxHp: number;
  enemyHp: number;
  enemyMaxHp: number;
  battleLog: string[];
  battleEnded: boolean;
  gainedXp?: number;
  newExperiencePoints?: number;
  playerLevel?: number;
  userXp?: number;
  userLevel?: number;
  playerEnergy?: number;
  enemyName: string; 
}

export interface BattleLogEntry {
  message: string;
  type: string; 
}