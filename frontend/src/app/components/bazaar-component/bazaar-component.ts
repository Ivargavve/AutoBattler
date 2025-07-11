import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface Vendor {
  id: number;
  name: string;
  minLevel: number;
  avatarUrl?: string;
}

interface Item {
  id: number;
  name: string;
  type: string;
  description?: string;
  iconUrl?: string;
  price: number;
  quantity?: number;
}

@Component({
  selector: 'app-bazaar-component',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './bazaar-component.html',
  styleUrl: './bazaar-component.scss'
})
export class BazaarComponent {
  // --- DUMMY DATA ---
  defaultVendor = 'https://cdn-icons-png.flaticon.com/512/1946/1946429.png';
  fallbackIcon = 'https://cdn-icons-png.flaticon.com/512/1177/1177568.png';

  vendors: Vendor[] = [
    { id: 1, name: 'Arno the Merchant', minLevel: 1, avatarUrl: this.defaultVendor },
    { id: 2, name: 'Elise the Alchemist', minLevel: 5, avatarUrl: this.defaultVendor },
    { id: 3, name: 'Bjorn the Blacksmith', minLevel: 10, avatarUrl: this.defaultVendor }
  ];
  selectedVendor: Vendor = this.vendors[0];

  allItems: Item[] = [
    { id: 1, name: 'Health Potion', type: 'Potion', price: 50, description: 'Restores 50 HP', iconUrl: '', quantity: 0 },
    { id: 2, name: 'Iron Sword', type: 'Weapon', price: 200, description: 'Basic melee weapon', iconUrl: '', quantity: 0 },
    { id: 3, name: 'Leather Armor', type: 'Armor', price: 120, description: 'Lightweight armor', iconUrl: '', quantity: 0 },
    { id: 4, name: 'Mana Potion', type: 'Potion', price: 75, description: 'Restores 30 MP', iconUrl: '', quantity: 0 },
    { id: 5, name: 'Elixir of Luck', type: 'Potion', price: 350, description: 'Boosts loot chance', iconUrl: '', quantity: 0 }
  ];
  filteredItems: Item[] = [...this.allItems];
  searchQuery = '';

  // Dummy inventory (for selling)
  playerInventory: Item[] = [
    { id: 1, name: 'Health Potion', type: 'Potion', price: 50, quantity: 3 },
    { id: 2, name: 'Iron Sword', type: 'Weapon', price: 200, quantity: 1 }
  ];
  playerGold = 1250;

  // --- METHODS FOR HTML ---

  selectVendor(v: Vendor) {
    this.selectedVendor = v;
    // Example: filter items by vendor level (simple demo)
    this.filteredItems = this.allItems.filter(item => v.minLevel <= this.selectedVendor.minLevel);
    // You can customize: e.g., each vendor shows different item pool
  }

  searchItems() {
    const q = this.searchQuery.toLowerCase();
    this.filteredItems = this.allItems.filter(item =>
      item.name.toLowerCase().includes(q) ||
      item.type.toLowerCase().includes(q)
    );
  }

  buyItem(item: Item) {
    if (this.playerGold >= item.price) {
      this.playerGold -= item.price;
      // Find if item exists in inventory, else add new
      let inv = this.playerInventory.find(i => i.id === item.id);
      if (inv) inv.quantity! += 1;
      else this.playerInventory.push({ ...item, quantity: 1 });
      // feedback/snackbar kan läggas till
    }
  }

  sellPrice(item: Item): number {
    return Math.floor(item.price * 0.6); // 60% av priset som exempel
  }

  sellItem(item: Item) {
    if (item.quantity && item.quantity > 0) {
      this.playerGold += this.sellPrice(item);
      item.quantity -= 1;
      if (item.quantity === 0) {
        // Ta bort från inventory
        this.playerInventory = this.playerInventory.filter(i => i.id !== item.id);
      }
    }
  }
}
