import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Fighter, BattleLogEntry } from '../../services/battle-interfaces';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-battle',
  standalone: true,
  imports: [ CommonModule ],
  templateUrl: './battle-component.html',
  styleUrl: './battle-component.scss'
})
export class BattleComponent implements OnInit, AfterViewInit {
  player: Fighter | null = null;
  enemy: Fighter | null = null;
  battleLog: BattleLogEntry[] = [];
  isLoading = false;
  battleEnded = false;
  gainedXp: number | null = null;
  userLevel: number | null = null;
  userXp: number | null = null;
  playerEnergy: number = 0;
  enemyName: string | null = null;

  @ViewChild('battleLogContainer') battleLogContainer!: ElementRef;

  constructor(private http: HttpClient, private authService: AuthService) {}

  ngOnInit(): void {
    this.loadPlayerData();
  }

  ngAfterViewInit(): void {
    this.scrollToBottom();
  }

  scrollToBottom(): void {
    setTimeout(() => {
      if (this.battleLogContainer && this.battleLogContainer.nativeElement) {
        this.battleLogContainer.nativeElement.scrollTo({
          top: this.battleLogContainer.nativeElement.scrollHeight,
          behavior: 'smooth'
        });
      }
    }, 30);
  }

  loadPlayerData(): void {
    this.isLoading = true;
    this.http.get<any>(`${environment.apiUrl}/characters/me`).subscribe({
      next: (playerData) => {
        this.player = {
          name: playerData.name,
          hp: playerData.currentHealth,
          maxHp: playerData.maxHealth
        };
        this.playerEnergy = playerData.currentEnergy;
        this.userLevel = playerData.level;
        this.userXp = playerData.experiencePoints;
        this.battleLog = [];
        this.battleEnded = true; 
        this.isLoading = false;
        this.scrollToBottom();
      },
      error: (err) => {
        this.battleLog = [{ message: "ERROR: " + (err.error?.message || err.statusText), type: "error" }];
        this.isLoading = false;
        this.scrollToBottom();
      }
    });
  }

  startNewBattle(): void {
    if (this.playerEnergy > 0) {
      this.isLoading = true;
      this.battleEnded = false;
      this.enemyName = null;
      this.enemy = null;
      this.battleLog = [{ message: "Starting new battle...", type: "start" }];
      this.scrollToBottom();

      this.http.get<any>(`${environment.apiUrl}/battle/encounter`).subscribe({
        next: (enemyData) => {
          this.enemyName = enemyData.enemyName;
          this.enemy = {
            name: enemyData.enemyName,
            hp: enemyData.enemyHp,
            maxHp: enemyData.enemyMaxHp
          };
          this.battleLog.push({ message: `You encounter a wild ${this.enemyName}! Prepare for battle!`, type: "encounter" });
          this.isLoading = false;
          this.scrollToBottom();
        },
        error: (err) => {
          this.battleLog = [{ message: "ERROR: " + (err.error?.message || err.statusText), type: "error" }];
          this.isLoading = false;
          this.scrollToBottom();
        }
      });

    } else {
      this.battleLog.push({ message: "You have no energy left to battle! Please rest or visit the shop.", type: "info" });
      this.battleEnded = true;
      this.scrollToBottom();
    }
  }

  attack(): void {
    if (!this.player || !this.enemy || this.battleEnded || this.playerEnergy <= 0) return;

    this.isLoading = true;

    const req = {
      enemyHp: this.enemy.hp,
      enemyName: this.enemy.name, 
      action: 'attack'
    };

    this.http.post<any>(`${environment.apiUrl}/battle/turn`, req)
      .subscribe({
        next: (res) => {
          this.player!.hp = res.playerHp;
          this.player!.maxHp = res.playerMaxHp;
          this.enemy!.hp = res.enemyHp;
          this.enemy!.maxHp = res.enemyMaxHp;
          this.enemy!.name = res.enemyName; 
          this.enemyName = res.enemyName;
          this.battleLog.push(...res.battleLog);
          this.battleEnded = res.battleEnded;

          if (res.battleEnded && this.player!.hp > 0) {
            this.gainedXp = res.gainedXp ?? null;
            this.userLevel = res.userLevel ?? this.userLevel;
            this.userXp = res.newExperiencePoints ?? this.userXp;
            this.playerEnergy = res.playerEnergy ?? this.playerEnergy;
            this.authService.loadUserWithCharacter();
          } else {
            this.gainedXp = null;
          }
          this.isLoading = false;
          this.scrollToBottom();
        },
        error: (err) => {
          this.battleLog.push({ message: "ERROR: " + (err.error?.message || err.statusText), type: "error" });
          this.isLoading = false;
          this.scrollToBottom();
        }
      });
  }

  getLogClass(log: BattleLogEntry): string {
    switch (log.type) {
      case 'victory':        return 'battle-victory';
      case 'defeat':         return 'battle-defeat';
      case 'player-crit':    return 'battle-crit';
      case 'enemy-crit':     return 'battle-crit-enemy';
      case 'player-attack-damage': return 'battle-friendly-damage';
      case 'enemy-attack-damage':  return 'battle-enemy-damage';
      case 'player-crit-damage':   return 'battle-crit';
      case 'enemy-crit-damage':    return 'battle-crit-enemy';
      case 'status':         return 'battle-status';
      case 'turn-end':       return 'battle-divider';
      case 'hp-row':         return 'battle-hp-row';
      case 'xp':             return 'battle-xp';
      case 'levelup':        return 'battle-levelup';
      case 'user-levelup':   return 'battle-user-levelup';
      case 'enemy-hp':       return 'battle-enemy-hp';
      case 'player-hp':      return 'battle-player-hp';
      case 'encounter':      return 'battle-encounter';
      default:               return '';
    }
  }
}
