import { Component, OnInit } from '@angular/core';

export interface Character {
  name: string;
  class: string;
  level: number;
  experiencePoints: number;
  avatarUrl?: string;
}

export interface GearItem {
  name: string;
  iconUrl?: string;
  stats: string;
}

export interface EquippedSlot {
  label: string;
  item: GearItem | null;
}

export interface InventoryItem {
  name: string;
  iconUrl?: string;
  stats: string;
  type: string;
  quantity: number;
}

@Component({
  selector: 'app-vault-component',
  templateUrl: './vault-component.html',
  styleUrls: ['./vault-component.scss'],
  standalone: true,
  imports: [],
})
export class VaultComponent implements OnInit {
  character: Character = {
    name: 'Wizz',
    class: 'Warrior',
    level: 17,
    experiencePoints: 14950,
    avatarUrl: '/assets/avatars/wizz.png',
  };

  equippedSlots: EquippedSlot[] = [
    { label: 'Head', item: { name: 'Iron Helm', iconUrl: '/assets/items/iron-helm.png', stats: '+3 Armor' } },
    { label: 'Chest', item: { name: 'Chainmail', iconUrl: '/assets/items/chainmail.png', stats: '+6 Armor' } },
    { label: 'Weapon', item: { name: 'Longsword', iconUrl: '/assets/items/longsword.png', stats: '+10 Attack' } },
    { label: 'Ring', item: null }
  ];

  inventory: InventoryItem[] = [
    { name: 'Health Potion', iconUrl: '/assets/items/potion.png', stats: 'Restore 50 HP', type: 'Consumable', quantity: 2 },
    { name: 'Mana Potion', iconUrl: '', stats: 'Restore 25 MP', type: 'Consumable', quantity: 1 },
    { name: 'Wooden Shield', iconUrl: '/assets/items/shield.png', stats: '+2 Armor', type: 'Off-Hand', quantity: 1 }
  ];

  defaultAvatar = '/assets/default-avatar.png';
  fallbackIcon = '/assets/fallback-item.png';

  totalItems = 0;
  uniqueGearCount = 0;

  ngOnInit() {
    this.totalItems = this.inventory.length;
    // Unique name count
    this.uniqueGearCount = new Set(this.inventory.map(i => i.name)).size;
  }
}
