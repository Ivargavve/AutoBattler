import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../services/user';
import { Character } from '../../services/character';
import { StatsService, EnhancedStats } from '../../services/stats.service';
import { Observable, of, map, switchMap } from 'rxjs';
import { LoadingSpinnerComponent } from '../loading-component/loading-component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, LoadingSpinnerComponent],
  templateUrl: './profile-component.html',
  styleUrls: ['./profile-component.scss']
})
export class ProfileComponent implements OnInit {
  user$: Observable<User | null> = of(null);
  character$: Observable<Character | null> = of(null);
  achievementsCount$: Observable<number> = of(0);
  cosmeticsCount$: Observable<number> = of(0);

  constructor(
    private authService: AuthService,
    private route: ActivatedRoute,
    private statsService: StatsService
  ) {}

  ngOnInit() {
    this.user$ = this.route.paramMap.pipe(
      switchMap(params => {
        const username = params.get('username');
        if (username) {
          return this.authService.getProfileByUsername(username);
        } else {
          return this.authService.user$;
        }
      })
    );

    this.character$ = this.route.paramMap.pipe(
      switchMap(params => {
        const username = params.get('username');
        if (username) {
          return this.authService.getCharacterByUsername(username).pipe(
            map(char => char));
        } else {
          return this.authService.character$;
        }
      })
    );

    this.achievementsCount$ = this.user$.pipe(
      map(user => {
        if (!user) return 0;
        try {
          const ach = JSON.parse(user.achievementsJson || '{}');
          return Object.keys(ach).length;
        } catch {
          return 0;
        }
      })
    );

    this.cosmeticsCount$ = this.user$.pipe(
      map(user => {
        if (!user) return 0;
        try {
          const cos = JSON.parse(user.cosmeticItemsJson || '{}');
          return Object.keys(cos).length;
        } catch {
          return 0;
        }
      })
    );
  }

  // Enhanced stats methods using the stats service
  getEnhancedStats(character: Character): EnhancedStats {
    return this.statsService.calculateTotalStats(character);
  }

  getStatWithBonus(character: Character, statName: keyof EnhancedStats): number {
    return this.statsService.getStatWithBonus(character, statName);
  }

  getEquipmentBonus(character: Character, statName: string): number {
    return this.statsService.getEquipmentBonus(character, statName);
  }

  formatStatDisplay(character: Character, statName: keyof EnhancedStats): {
    base: number;
    bonus: number;
    total: number;
  } {
    return this.statsService.formatStatDisplay(character, statName);
  }

  formatStatDisplayForUI(character: Character, statName: keyof EnhancedStats): {
    total: number;
    bonus: number;
    hasBonus: boolean;
  } {
    return this.statsService.formatStatDisplayForUI(character, statName);
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
}
