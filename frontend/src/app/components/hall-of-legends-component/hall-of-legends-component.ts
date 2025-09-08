import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TopCharactersService, TopCharactersData, TopCharacterEntry } from '../../services/top-characters.service';

@Component({
  selector: 'app-hall-of-legends-component',
  imports: [CommonModule],
  templateUrl: './hall-of-legends-component.html',
  styleUrl: './hall-of-legends-component.scss'
})
export class HallOfLegendsComponent implements OnInit {
  topCharactersData: TopCharactersData | null = null;
  loading = true;
  error: string | null = null;

  constructor(private topCharactersService: TopCharactersService) {}

  ngOnInit() {
    this.loadTopCharacters();
  }

  loadTopCharacters() {
    this.loading = true;
    this.error = null;
    
    this.topCharactersService.getTopCharacters().subscribe({
      next: (data) => {
        this.topCharactersData = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading top characters:', error);
        this.error = 'Failed to load hall of legends data';
        this.loading = false;
      }
    });
  }

  getStatDescription(entry: TopCharacterEntry): string {
    switch (entry.statName) {
      case 'Level':
        return `Level ${entry.statValue} - ${entry.title}`;
      case 'Attack':
        return `Attack Power: ${entry.statValue} - ${entry.title}`;
      case 'Health':
        return `Max Health: ${entry.statValue} - ${entry.title}`;
      case 'Defense':
        return `Defense: ${entry.statValue} - ${entry.title}`;
      case 'Agility':
        return `Agility: ${entry.statValue} - ${entry.title}`;
      case 'Magic':
        return `Magic Power: ${entry.statValue} - ${entry.title}`;
      default:
        return `${entry.statName}: ${entry.statValue}`;
    }
  }

  getCategoryTitle(category: string): string {
    switch (category) {
      case 'kingOfAutobattler':
        return 'King of Autobattler';
      case 'attackMasters':
        return 'Attack Masters';
      case 'tankyBankies':
        return 'Tanky Bankies';
      case 'defenseChampions':
        return 'Defense Champions';
      case 'speedDemons':
        return 'Speed Demons';
      case 'magicWielders':
        return 'Magic Wielders';
      default:
        return category;
    }
  }

  getCategoryIcon(category: string): string {
    switch (category) {
      case 'kingOfAutobattler':
        return '👑';
      case 'attackMasters':
        return '⚔️';
      case 'tankyBankies':
        return '🛡️';
      case 'defenseChampions':
        return '🏰';
      case 'speedDemons':
        return '⚡';
      case 'magicWielders':
        return '🔮';
      default:
        return '⭐';
    }
  }
}
