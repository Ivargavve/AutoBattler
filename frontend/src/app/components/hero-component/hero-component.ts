import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { Character } from '../../services/character';
import { Observable } from 'rxjs';
import { tap, take } from 'rxjs/operators';

type StatKey = 'attack' | 'defense' | 'agility' | 'magic' | 'speed';

const STAT_KEYS: StatKey[] = ['attack', 'defense', 'agility', 'magic', 'speed'];

@Component({
  selector: 'app-hero',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hero-component.html',
  styleUrls: ['./hero-component.scss']
})
export class HeroComponent implements OnInit {
  character$: Observable<Character | null>;
  
  // Expose STAT_KEYS for template
  readonly statKeys = STAT_KEYS;

  editMode = false;
  pointsRemaining = 0;
  saving = false;
  readonly HP_PER_POINT = 5;

  private availablePoints = 0;

  originalStats: Record<StatKey, number> = {
    attack: 0,
    defense: 0,
    agility: 0,
    magic: 0,
    speed: 0
  };

  editedStats: Record<StatKey, number> = {
    attack: 0,
    defense: 0,
    agility: 0,
    magic: 0,
    speed: 0
  };

  originalMaxHealth = 0;
  editedMaxHealth = 0;

  constructor(
    private authService: AuthService,
    private route: ActivatedRoute
  ) {
    this.character$ = this.authService.character$;
  }

  ngOnInit(): void {
    const allocate = this.route.snapshot.queryParamMap.get('allocate');
    if (allocate === '1') {
      this.authService.character$
        .pipe(
          take(1),
          tap(c => c && this.enterAllocateMode(c))
        )
        .subscribe();
    }
  }

  private getUnspent(character: Character): number {
    // Stöd både camelCase och PascalCase från backend
    const camel = (character as any).unspentStatPoints;
    const pascal = (character as any).UnspentStatPoints;
    return typeof camel === 'number' ? camel : (typeof pascal === 'number' ? pascal : 0);
  }

  enterAllocateMode(character: Character): void {
    const unspent = this.getUnspent(character);
    if (unspent <= 0) return;

    this.editMode = true;

    // Tillgängliga poäng denna allokering = alla ospenderade
    this.availablePoints = unspent;
    this.pointsRemaining = this.availablePoints;

    this.originalStats = {
      attack: character.attack,
      defense: character.defense,
      agility: character.agility,
      magic: (character as any).magic ?? 0,
      speed: (character as any).speed ?? 0
    };
    this.editedStats = { ...this.originalStats };

    this.originalMaxHealth = character.maxHealth;
    this.editedMaxHealth = character.maxHealth;
  }

  cancel(): void {
    this.editMode = false;
    this.pointsRemaining = 0;
    this.availablePoints = 0;
  }

  allocated(key: StatKey): number {
    return this.editedStats[key] - this.originalStats[key];
  }

  allocatedHpPoints(): number {
    return Math.floor((this.editedMaxHealth - this.originalMaxHealth) / this.HP_PER_POINT);
  }

  private totalAllocated(): number {
    return (
      this.allocated('attack') +
      this.allocated('defense') +
      this.allocated('agility') +
      this.allocated('magic') +
      this.allocated('speed') +
      this.allocatedHpPoints()
    );
  }

  private recomputePointsLeft(): void {
    this.pointsRemaining = this.availablePoints - this.totalAllocated();
  }

  increment(key: StatKey | 'hp'): void {
    if (this.pointsRemaining <= 0) return;

    if (key === 'hp') {
      this.editedMaxHealth += this.HP_PER_POINT;
    } else {
      this.editedStats[key]++;
    }

    this.recomputePointsLeft();
  }

  decrement(key: StatKey | 'hp'): void {
    if (key === 'hp') {
      if (this.allocatedHpPoints() <= 0) return;
      this.editedMaxHealth -= this.HP_PER_POINT;
    } else {
      if (this.allocated(key) <= 0) return;
      this.editedStats[key]--;
    }

    this.recomputePointsLeft();
  }

  save(character: Character): void {
    if (this.pointsRemaining !== 0 || this.saving) return;

    const payload = {
      attack: this.editedStats.attack,
      defense: this.editedStats.defense,
      agility: this.editedStats.agility,
      magic: this.editedStats.magic,
      speed: this.editedStats.speed,
      maxHealth: this.editedMaxHealth
    };

    this.saving = true;

    this.authService.updateCharacterStats(payload).subscribe({
      next: () => {
        this.saving = false;
        this.editMode = false;
        // character$ uppdateras via AuthService.tap() i updateCharacterStats
      },
      error: err => {
        console.error('save stats error:', err?.error?.message ?? err?.error ?? err);
        this.saving = false;
      }
    });
  }

  getStatIcon(stat: string): string {
    const icons: { [key: string]: string } = {
      'attack': '⚔️',
      'defense': '🛡️',
      'agility': '💨',
      'magic': '🔮',
      'speed': '🏃'
    };
    return icons[stat] || '📊';
  }
}
