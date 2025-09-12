import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { Character } from '../../services/character';
import { PlayerAttack } from '../../services/battle-interfaces';
import { StatsService } from '../../services/stats.service';
import { Observable, Subscription, of } from 'rxjs';
import { TitleService } from '../../services/title.service';

@Component({
  selector: 'app-vault-component',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vault-component.html',
  styleUrls: ['./vault-component.scss'],
})
export class VaultComponent implements OnInit, OnDestroy {
  character: Character | null = null;
  characterSub!: Subscription;
  inventory: any[] = [];
  equippedSlots: { slot: string, itemId?: number }[] = [];
  attacks: PlayerAttack[] = [];

  defaultAvatar = '/assets/default-avatar.png';
  fallbackIcon = '/assets/fallback-item.png';

  totalItems = 0;
  uniqueGearCount = 0;

  constructor(
    private authService: AuthService,
    private statsService: StatsService,
    private titleService: TitleService
  ) {}

  ngOnInit() {
    this.titleService.setTitle('Vault');
    this.characterSub = this.authService.character$.subscribe(character => {
      if (character) {
        this.character = character;
        try {
          this.inventory = JSON.parse(character.inventoryJson || '[]');
        } catch {
          this.inventory = [];
        }

        try {
          const raw = JSON.parse(character.equipmentJson || '[]');
          this.equippedSlots = Array.isArray(raw) ? raw.map((e: any) => ({ slot: (e.slot || e.Slot || '').toLowerCase(), itemId: e.itemId ?? e.ItemId })) : [];
        } catch {
          this.equippedSlots = [];
        }

        // Normalize inventory items to a consistent shape
        this.inventory = (this.inventory || []).map((it: any) => ({
          id: it.Id ?? it.id,
          name: it.Name ?? it.name,
          type: it.Type ?? it.type,
          slot: (it.Slot ?? it.slot ?? '').toLowerCase(),
          rarity: it.Rarity ?? it.rarity,
          imageUrl: (it.ImageUrl ?? it.imageUrl) || '',
          statBonuses: it.StatBonuses ?? it.statBonuses ?? {},
          quantity: it.Quantity ?? it.quantity ?? 1
        }));

        // Ensure all known slots exist even if unequipped
        const knownSlots = ['head','weapon','chest','legs','arms','hands','feet','belt','back','ring','amulet'];
        const map: Record<string, number|undefined> = {};
        for (const e of this.equippedSlots) map[e.slot] = e.itemId;
        this.equippedSlots = knownSlots.map(s => ({ slot: s, itemId: map[s] }));

        if (character.attacks && character.attacks.length > 0) {
          this.attacks = character.attacks;
        } else if (character.attacksJson) {
          try {
            const rawAttacks = JSON.parse(character.attacksJson);
            this.attacks = rawAttacks.map((atk: any) => ({
              id: atk.Id ?? atk.id,
              name: atk.Name ?? atk.name,
              type: atk.Type ?? atk.type,
              damageType: atk.DamageType ?? atk.damageType,
              baseDamage: atk.BaseDamage ?? atk.baseDamage,
              maxCharges: atk.MaxCharges ?? atk.maxCharges,
              currentCharges: atk.CurrentCharges ?? atk.currentCharges,
              scaling: atk.Scaling ?? atk.scaling ?? {},
              requiredStats: atk.RequiredStats ?? atk.requiredStats ?? {},
              allowedClasses: atk.AllowedClasses ?? atk.allowedClasses ?? [],
              description: atk.Description ?? atk.description ?? '',
            }));
          } catch {
            this.attacks = [];
          }
        } else {
          this.attacks = [];
        }

        this.totalItems = this.inventory.reduce((sum, item) => sum + (item.quantity || 1), 0);
        this.uniqueGearCount = new Set(this.inventory.map(i => i.name)).size;
      } else {
        this.character = null;
        this.inventory = [];
        this.equippedSlots = [];
        this.attacks = [];
        this.totalItems = 0;
        this.uniqueGearCount = 0;
      }
    });
  }

  ngOnDestroy() {
    if (this.characterSub) this.characterSub.unsubscribe();
    this.titleService.setBaseTitle();
  }

  onImageError(event: any) {
    const currentSrc = event.target.src;
    if (!currentSrc.includes('assets/characters/') && currentSrc.includes('char')) {
      const match = currentSrc.match(/char\d+\.jpeg/);
      if (match) {
        const fixedSrc = `assets/characters/${match[0]}`;
        event.target.src = fixedSrc;
        return;
      }
    }
    event.target.src = 'assets/characters/char1.jpeg';
  }

  // Use the centralized stats service methods
  getBaseStat(stat: string): number {
    if (!this.character) return 0;
    return this.statsService.getBaseStats(this.character)[stat as keyof import('../../services/stats.service').EnhancedStats] || 0;
  }

  getBonusStat(stat: string): number {
    if (!this.character) return 0;
    return this.statsService.getEquipmentBonus(this.character, stat);
  }

  getTotalStat(stat: string): number {
    if (!this.character) return 0;
    const baseStats = this.statsService.getBaseStats(this.character);
    const bonus = this.getBonusStat(stat);
    const statKey = stat === 'maxHealth' ? 'maxHealth' : stat;
    return baseStats[statKey as keyof import('../../services/stats.service').EnhancedStats] + bonus;
  }

  formatStatDisplayForUI(stat: string): {
    total: number;
    bonus: number;
    hasBonus: boolean;
  } {
    if (!this.character) return { total: 0, bonus: 0, hasBonus: false };
    const statKey = stat === 'maxHealth' ? 'maxHealth' : stat;
    return this.statsService.formatStatDisplayForUI(this.character, statKey as keyof import('../../services/stats.service').EnhancedStats);
  }

  // Get inventory items that are NOT equipped
  get unequippedInventory() {
    const equippedItemIds = this.equippedSlots
      .filter(slot => slot.itemId)
      .map(slot => slot.itemId);
    
    return this.inventory.filter(item => 
      !equippedItemIds.includes(item.id)
    );
  }

  getSlotLabel(slot: string) {
    const map: Record<string, string> = {
      head: 'Head',
      chest: 'Chest',
      legs: 'Legs',
      arms: 'Arms',
      hands: 'Hands',
      feet: 'Feet',
      back: 'Back',
      belt: 'Belt',
      ring: 'Ring',
      amulet: 'Amulet',
      weapon: 'Weapon'
    };
    return map[slot] || slot;
  }

  itemsForSlot(slot: string) {
    return this.inventory.filter(i => i.slot === slot);
  }

  getEquippedItem(slot: string) {
    const es = this.equippedSlots.find(s => s.slot === slot);
    if (!es?.itemId) return null;
    return this.inventory.find(i => i.id === es.itemId) || null;
  }

  equip(slot: string, itemId: number | string) {
    const id = Number(itemId);
    if (!id) return;
    this.authService.equipItem(slot, id).subscribe();
  }

  unequip(slot: string) {
    this.authService.unequipItem(slot).subscribe();
  }

  // Abilities: pick up to 4
  get ownedAttacks(): PlayerAttack[] {
    return this.attacks || [];
  }

  get equippedAttackIds(): number[] {
    try {
      const raw = JSON.parse(this.character?.attacksJson || '[]');
      return raw.filter((a: any) => !!(a.Equipped ?? a.equipped)).map((a: any) => a.Id ?? a.id);
    } catch {
      return [];
    }
  }

  toggleEquipAttack(id: number) {
    const set = new Set(this.equippedAttackIds);
    if (set.has(id)) set.delete(id); else set.add(id);
    const next = Array.from(set).slice(0, 4);
    this.authService.equipAbilities(next).subscribe();
  }

  changeAbilityAtIndex(index: number, value: string) {
    const current = [...this.equippedAttackIds];
    if (!value) {
      current[index] = undefined as any;
    } else {
      current[index] = Number(value);
    }
    // Ensure uniqueness and max 4
    const cleaned = current.filter(v => !!v) as number[];
    const unique = Array.from(new Set(cleaned)).slice(0, 4);
    this.authService.equipAbilities(unique).subscribe();
  }
}
