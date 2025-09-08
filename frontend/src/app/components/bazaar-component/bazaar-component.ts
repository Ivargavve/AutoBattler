import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TitleService } from '../../services/title.service';
import { AttackShopService, AttackShopItem } from '../../services/attack-shop.service';
import { ItemShopService, ItemShopItem } from '../../services/item-shop.service';
import { AuthService } from '../../services/auth.service';
import { Character } from '../../services/character';
import { LoadingSpinnerComponent } from '../loading-component/loading-component';


@Component({
  selector: 'app-bazaar-component',
  standalone: true,
  imports: [FormsModule, CommonModule, LoadingSpinnerComponent],
  templateUrl: './bazaar-component.html',
  styleUrl: './bazaar-component.scss'
})
export class BazaarComponent implements OnInit, OnDestroy {
  // Shop properties
  activeTab: string = 'abilities';
  character: Character | null = null;
  loading: boolean = false;
  error: string | null = null;

  // Attack shop properties
  availableAttacks: AttackShopItem[] = [];
  filteredAttacks: AttackShopItem[] = [];
  attackSortBy: string = 'price';
  attackFilterClass: string = '';
  attacksLoading: boolean = false;

  // Item shop properties
  availableItems: ItemShopItem[] = [];
  filteredItems: ItemShopItem[] = [];
  itemSortBy: string = 'price';
  itemFilterSlot: string = '';
  itemFilterRarity: string = '';
  itemsLoading: boolean = false;

  constructor(
    private titleService: TitleService,
    private attackShopService: AttackShopService,
    private itemShopService: ItemShopService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.titleService.setTitle('Bazaar');
    this.loadAvailableAttacks();
    this.loadAvailableItems();
    
    // Subscribe to character changes
    this.authService.character$.subscribe(char => {
      this.character = char;
      if (char) {
        this.loadAvailableAttacks();
        this.loadAvailableItems();
      }
    });
  }

  ngOnDestroy() {
    this.titleService.setBaseTitle();
  }

  // Attack shop methods
  loadAvailableAttacks() {
    this.attacksLoading = true;
    this.error = null;
    
    this.attackShopService.getAvailableAttacks().subscribe({
      next: (attacks) => {
        this.availableAttacks = attacks;
        this.filterAttacks();
        this.attacksLoading = false;
      },
      error: (error) => {
        console.error('Error loading attacks:', error);
        this.error = 'Failed to load abilities. Please try again.';
        this.attacksLoading = false;
      }
    });
  }

  filterAttacks() {
    let filtered = [...this.availableAttacks];

    // Filter by class
    if (this.attackFilterClass) {
      filtered = filtered.filter(attack => 
        attack.allowedClasses.some(cls => cls.toLowerCase() === this.attackFilterClass.toLowerCase())
      );
    }

    // Sort attacks
    filtered.sort((a, b) => {
      switch (this.attackSortBy) {
        case 'name':
          return a.name.localeCompare(b.name);
        case 'requirements':
          const aReqs = Object.keys(a.requiredStats).length;
          const bReqs = Object.keys(b.requiredStats).length;
          return aReqs - bReqs;
        case 'price':
        default:
          return a.price - b.price;
      }
    });

    this.filteredAttacks = filtered;
  }

  onAttackSortChange() {
    this.filterAttacks();
  }

  onAttackClassFilterChange() {
    this.filterAttacks();
  }

  getRequirementEntries(requiredStats: { [key: string]: number }) {
    return Object.entries(requiredStats);
  }

  purchaseAttack(attack: AttackShopItem) {
    if (!attack.canAfford || !attack.meetsRequirements) {
      return;
    }

    this.attackShopService.purchaseAttack(attack.id).subscribe({
      next: (response) => {
        console.log('Attack purchased:', response);
        // Character data will be updated automatically via the character$ observable
        this.loadAvailableAttacks();
        alert(`Successfully purchased ${attack.name}! Remaining credits: ${response.remainingCredits}`);
      },
      error: (error) => {
        console.error('Error purchasing attack:', error);
        alert('Failed to purchase attack. Please try again.');
      }
    });
  }

  // Item shop methods
  loadAvailableItems() {
    this.itemsLoading = true;
    this.error = null;
    
    this.itemShopService.getAvailableItems().subscribe({
      next: (items) => {
        this.availableItems = items;
        this.filterItems();
        this.itemsLoading = false;
      },
      error: (error) => {
        console.error('Error loading items:', error);
        this.error = 'Failed to load items. Please try again.';
        this.itemsLoading = false;
      }
    });
  }

  filterItems() {
    let filtered = [...this.availableItems];

    // Filter by slot
    if (this.itemFilterSlot) {
      filtered = filtered.filter(item => item.slot === this.itemFilterSlot);
    }

    // Filter by rarity
    if (this.itemFilterRarity) {
      filtered = filtered.filter(item => item.rarity === this.itemFilterRarity);
    }

    // Sort items
    filtered.sort((a, b) => {
      switch (this.itemSortBy) {
        case 'name':
          return a.name.localeCompare(b.name);
        case 'rarity':
          const rarityOrder = { 'common': 1, 'uncommon': 2, 'rare': 3, 'epic': 4, 'legendary': 5 };
          return (rarityOrder[a.rarity as keyof typeof rarityOrder] || 0) - (rarityOrder[b.rarity as keyof typeof rarityOrder] || 0);
        case 'level':
          return a.requiredLevel - b.requiredLevel;
        case 'price':
        default:
          return a.price - b.price;
      }
    });

    this.filteredItems = filtered;
  }

  onItemSortChange() {
    this.filterItems();
  }

  onItemSlotFilterChange() {
    this.filterItems();
  }

  onItemRarityFilterChange() {
    this.filterItems();
  }

  getStatBonusEntries(statBonuses: { [key: string]: number }) {
    return Object.entries(statBonuses);
  }

  purchaseItem(item: ItemShopItem) {
    if (!item.canAfford || !item.meetsRequirements) {
      return;
    }

    this.itemShopService.purchaseItem(item.id).subscribe({
      next: (response) => {
        console.log('Item purchased:', response);
        this.loadAvailableItems();
        alert(`Successfully purchased ${item.name}! Remaining credits: ${response.remainingCredits}`);
      },
      error: (error) => {
        console.error('Error purchasing item:', error);
        alert('Failed to purchase item. Please try again.');
      }
    });
  }

  getRarityColor(rarity: string): string {
    switch (rarity) {
      case 'common': return '#9ca3af';
      case 'uncommon': return '#10b981';
      case 'rare': return '#3b82f6';
      case 'epic': return '#8b5cf6';
      case 'legendary': return '#f59e0b';
      default: return '#9ca3af';
    }
  }
}
