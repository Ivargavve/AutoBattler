import { Injectable } from '@angular/core';
import { Character } from './character';

export interface EnhancedStats {
  attack: number;
  defense: number;
  agility: number;
  magic: number;
  speed: number;
  maxHealth: number;
  criticalChance: number;
}

export interface EquipmentBonuses {
  attack: number;
  defense: number;
  agility: number;
  magic: number;
  speed: number;
  health: number;
}

@Injectable({
  providedIn: 'root'
})
export class StatsService {

  /**
   * Calculate equipment bonuses from equipped items
   */
  calculateEquipmentBonuses(character: Character): EquipmentBonuses {
    const bonuses: EquipmentBonuses = {
      attack: 0,
      defense: 0,
      agility: 0,
      magic: 0,
      speed: 0,
      health: 0
    };

    try {
      // Parse equipped items
      const equippedSlots = JSON.parse(character.equipmentJson || '[]');
      const inventory = JSON.parse(character.inventoryJson || '[]');

      // Create map of equipped items
      const equippedItems = equippedSlots
        .filter((slot: any) => slot.itemId)
        .map((slot: any) => inventory.find((item: any) => item.Id === slot.itemId || item.id === slot.itemId))
        .filter(Boolean);

      // Sum up stat bonuses from all equipped items
      for (const item of equippedItems) {
        const statBonuses = item.StatBonuses || item.statBonuses || {};
        
        for (const [statName, value] of Object.entries(statBonuses)) {
          const statKey = statName.toLowerCase();
          if (statKey === 'health') {
            bonuses.health += Number(value) || 0;
          } else if (statKey in bonuses) {
            bonuses[statKey as keyof EquipmentBonuses] += Number(value) || 0;
          }
        }
      }
    } catch (error) {
      console.warn('Error calculating equipment bonuses:', error);
    }

    return bonuses;
  }

  /**
   * Get base stats from character (without equipment bonuses)
   */
  getBaseStats(character: Character): EnhancedStats {
    return {
      attack: character.attack,
      defense: character.defense,
      agility: character.agility,
      magic: character.magic || 0,
      speed: character.speed || 0,
      maxHealth: character.maxHealth,
      criticalChance: character.criticalChance
    };
  }

  /**
   * Calculate total stats (base + equipment bonuses)
   */
  calculateTotalStats(character: Character): EnhancedStats {
    const baseStats = this.getBaseStats(character);
    const equipmentBonuses = this.calculateEquipmentBonuses(character);

    return {
      attack: baseStats.attack + equipmentBonuses.attack,
      defense: baseStats.defense + equipmentBonuses.defense,
      agility: baseStats.agility + equipmentBonuses.agility,
      magic: baseStats.magic + equipmentBonuses.magic,
      speed: baseStats.speed + equipmentBonuses.speed,
      maxHealth: baseStats.maxHealth + equipmentBonuses.health,
      criticalChance: baseStats.criticalChance
    };
  }

  /**
   * Get individual stat with equipment bonus
   */
  getStatWithBonus(character: Character, statName: keyof EnhancedStats): number {
    const totalStats = this.calculateTotalStats(character);
    return totalStats[statName];
  }

  /**
   * Get equipment bonus for a specific stat
   */
  getEquipmentBonus(character: Character, statName: string): number {
    const bonuses = this.calculateEquipmentBonuses(character);
    const key = statName.toLowerCase() === 'maxhealth' ? 'health' : statName.toLowerCase();
    return bonuses[key as keyof EquipmentBonuses] || 0;
  }

  /**
   * Format stat display (base + bonus = total)
   */
  formatStatDisplay(character: Character, statName: keyof EnhancedStats): {
    base: number;
    bonus: number;
    total: number;
  } {
    const baseStats = this.getBaseStats(character);
    const bonus = this.getEquipmentBonus(character, statName);
    
    return {
      base: baseStats[statName],
      bonus: bonus,
      total: baseStats[statName] + bonus
    };
  }

  /**
   * Format stat display for UI (total | bonus) where bonus is green when > 0
   */
  formatStatDisplayForUI(character: Character, statName: keyof EnhancedStats): {
    total: number;
    bonus: number;
    hasBonus: boolean;
  } {
    const baseStats = this.getBaseStats(character);
    const bonus = this.getEquipmentBonus(character, statName);
    const total = baseStats[statName] + bonus;
    
    return {
      total: total,
      bonus: bonus,
      hasBonus: bonus > 0
    };
  }
}
