import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TopCharactersService, TopCharactersData, TopCharacterEntry } from '../../services/top-characters.service';
import { LoadingSpinnerComponent } from '../loading-component/loading-component';
import { TitleService } from '../../services/title.service';

@Component({
  selector: 'app-hall-of-legends-component',
  imports: [CommonModule, LoadingSpinnerComponent],
  templateUrl: './hall-of-legends-component.html',
  styleUrl: './hall-of-legends-component.scss'
})
export class HallOfLegendsComponent implements OnInit, OnDestroy {
  topCharactersData: TopCharactersData | null = null;
  loading = true;
  error: string | null = null;

  constructor(
    private topCharactersService: TopCharactersService,
    private titleService: TitleService
  ) {}

  ngOnInit() {
    this.titleService.setTitle('Hall of Legends');
    this.loadTopCharacters();
  }

  ngOnDestroy() {
    this.titleService.setBaseTitle();
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

  onImageError(event: any) {
    console.log('Character image failed to load:', event.target.src);
    
    // If the image path doesn't start with 'assets/', try to fix it
    const currentSrc = event.target.src;
    if (!currentSrc.includes('assets/characters/') && currentSrc.includes('char')) {
      // Extract the character filename (e.g., 'char11.jpeg')
      const match = currentSrc.match(/char\d+\.jpeg/);
      if (match) {
        const fixedSrc = `assets/characters/${match[0]}`;
        console.log('Trying fixed path:', fixedSrc);
        event.target.src = fixedSrc;
        return;
      }
    }
    
    // Set fallback image
    event.target.src = 'assets/characters/char1.jpeg';
  }
}
