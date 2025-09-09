import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { TitleService } from '../../services/title.service';
import { AuthService } from '../../services/auth.service';
import { Character } from '../../services/character';
import { TopCharactersService } from '../../services/top-characters.service';
import { environment } from '../../../environments/environment';
import { Observable, of, forkJoin } from 'rxjs';
import { catchError } from 'rxjs/operators';

interface RecommendedEnemy {
  name: string;
  level: number;
  type: string;
  description: string;
  imageUrl: string;
  difficulty: 'Easy' | 'Medium' | 'Hard';
  xp: number;
  credits: number;
}

interface KingData {
  characterName: string;
  userName: string;
  level: number;
  class: string;
  profileIconUrl: string;
  statValue: number;
  statName: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home-component.html',
  styleUrl: './home-component.scss'
})
export class HomeComponent implements OnInit, OnDestroy {
  character: Character | null = null;
  kingData: KingData | null = null;
  recommendedEnemies: RecommendedEnemy[] = [];
  loading = true;
  error: string | null = null;

  constructor(
    private titleService: TitleService,
    private authService: AuthService,
    private topCharactersService: TopCharactersService,
    private http: HttpClient,
    private router: Router
  ) {}

  ngOnInit() {
    this.titleService.setTitle('Home');
    this.loadData();
  }

  ngOnDestroy() {
    this.titleService.setBaseTitle();
  }

  loadData() {
    this.loading = true;
    this.error = null;

    // Subscribe to character changes
    this.authService.character$.subscribe(char => {
      this.character = char;
      if (char) {
        this.loadRecommendedEnemies(char.level);
      }
    });

    // Load king data
    this.loadKingData();
  }

  loadKingData() {
    this.topCharactersService.getTopCharacters().subscribe({
      next: (data) => {
        if (data.kingOfAutobattler && data.kingOfAutobattler.length > 0) {
          const king = data.kingOfAutobattler[0];
          this.kingData = {
            characterName: king.characterName,
            userName: king.userName,
            level: king.level,
            class: king.class,
            profileIconUrl: king.profileIconUrl,
            statValue: king.statValue,
            statName: king.statName
          };
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading king data:', error);
        this.error = 'Failed to load leaderboard data';
        this.loading = false;
      }
    });
  }

  loadRecommendedEnemies(playerLevel: number) {
    this.http.get<any[]>(`${environment.apiUrl}/enemies`).subscribe({
      next: (enemies) => {
        // Filter enemies based on player level and create recommendations
        const suitableEnemies = enemies
          .filter(enemy => enemy.level <= playerLevel + 2 && enemy.level >= Math.max(1, playerLevel - 1))
          .sort((a, b) => a.level - b.level)
          .slice(0, 3);

        this.recommendedEnemies = suitableEnemies.map(enemy => ({
          name: enemy.name,
          level: enemy.level,
          type: enemy.type,
          description: enemy.description,
          imageUrl: enemy.imageUrl || 'assets/monsters/skeleton.jpg',
          difficulty: this.getDifficulty(enemy.level, playerLevel),
          xp: enemy.xp,
          credits: Math.floor(Math.random() * (enemy.creditsMax - enemy.creditsMin + 1)) + enemy.creditsMin
        }));
      },
      error: (error) => {
        console.error('Error loading enemies:', error);
        this.error = 'Failed to load recommended enemies';
      }
    });
  }

  getDifficulty(enemyLevel: number, playerLevel: number): 'Easy' | 'Medium' | 'Hard' {
    const diff = enemyLevel - playerLevel;
    if (diff <= -1) return 'Easy';
    if (diff <= 1) return 'Medium';
    return 'Hard';
  }

  getDifficultyColor(difficulty: string): string {
    switch (difficulty) {
      case 'Easy': return '#4fff90';
      case 'Medium': return '#ffa500';
      case 'Hard': return '#ff4444';
      default: return '#888';
    }
  }

  fightEnemy(enemyName: string) {
    this.router.navigate(['/battle-hub'], { 
      queryParams: { enemy: enemyName } 
    });
  }

  goToBazaar() {
    this.router.navigate(['/bazaar']);
  }

  goToBattleHub() {
    this.router.navigate(['/battle-hub']);
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
