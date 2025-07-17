import { PlayerAttack } from './battle-interfaces';

export interface Character {
  id: number;
  name: string;
  class: string;
  profileIconUrl: string;

  level: number;
  experiencePoints: number;
  maxExperiencePoints: number;

  currentHealth: number;
  maxHealth: number;
  currentEnergy: number;
  maxEnergy: number;

  attack: number;
  defense: number;
  agility: number;
  criticalChance: number;

  credits: number;
  inventoryJson: string;
  equipmentJson: string;

  createdAt: string;
  updatedAt: string;
  lastRechargeTime?: string | Date;
  nextTickInSeconds?: number;

  attacksJson?: string;         
  attacks?: PlayerAttack[];       
}
