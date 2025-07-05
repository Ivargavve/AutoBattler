import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Fighter, BattleResponse } from '../../services/battle-interfaces';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-battle',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './battle-component.html',
  styleUrl: './battle-component.scss'
})
export class BattleComponent implements OnInit {
  player: Fighter | null = null;
  enemy: Fighter | null = null;
  battleLog: string[] = [];
  isLoading = false;
  battleEnded = false;
  gainedXp: number | null = null;
  userLevel: number | null = null;
  userXp: number | null = null;
  playerEnergy: number = 0;
  enemyName: string | null = null;

  constructor(private http: HttpClient, private authService: AuthService) {}

  formatBattleLog(log: string): string {
    let out = log;
    if (this.enemyName) {
      out = out.replace(
        new RegExp(`The attack deals (\\d+) damage to the ${this.enemyName.toLowerCase()}\\.`,"gi"),
        `<span class="log-damage-friend">The attack deals $1 damage to the ${this.enemyName}.</span>`
      );
      out = out.replace(
        new RegExp(`${this.player?.name} deals (\\d+) damage to the ${this.enemyName.toLowerCase()}`,"gi"),
        `<span class="log-damage-friend">${this.player?.name} deals $1 damage to the ${this.enemyName}.</span>`
      );
    }
    if (this.player?.name) {
      out = out.replace(
        new RegExp(`${this.player.name} takes (\\d+) damage`, "gi"),
        `<span class="log-damage-enemy">${this.player?.name} takes $1 damage</span>`
      );
    }
    out = out.replace(/Critical hit/gi, `<span class="log-crit">Critical hit</span>`)
      .replace(/Victory/gi, `<span class="log-victory">Victory</span>`)
      .replace(/defeated/gi, `<span class="log-defeat">defeated</span>`);
    return out;
  }

  isEnemyDamageLog(log: string): boolean {
    if (!this.player?.name) return false;
    return new RegExp(`${this.player.name} takes \\d+ damage`, 'i').test(log);
  }
  isFriendlyDamageLog(log: string): boolean {
    if (!this.player?.name) return false;
    return new RegExp(`${this.player.name} deals \\d+ damage`, 'i').test(log);
  }
  isCritLog(log: string): boolean {
    return /critical hit/i.test(log);
  }
  isVictoryLog(log: string): boolean {
    return /victory/i.test(log);
  }
  isDefeatLog(log: string): boolean {
    return /defeated/i.test(log);
  }

  ngOnInit(): void {
    this.loadPlayerData();
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
      },
      error: (err) => {
        this.battleLog = ["ERROR: " + (err.error?.message || err.statusText)];
        this.isLoading = false;
      }
    });
  }

  startNewBattle(): void {
    if (this.playerEnergy > 0) {
      this.isLoading = true;
      this.battleEnded = false;
      this.enemyName = null;
      this.enemy = null;
      this.battleLog = ["Starting new battle..."];

      this.http.get<any>(`${environment.apiUrl}/battle/encounter`).subscribe({
        next: (enemyData) => {
          this.enemyName = enemyData.enemyName;
          this.enemy = {
            name: enemyData.enemyName,
            hp: enemyData.enemyHp,
            maxHp: enemyData.enemyMaxHp
          };

          this.battleLog.push(`You encounter a wild ${this.enemyName}! Prepare for battle!`);
          this.isLoading = false;
        },
        error: (err) => {
          this.battleLog = ["ERROR: " + (err.error?.message || err.statusText)];
          this.isLoading = false;
        }
      });

    } else {
      this.battleLog.push("You have no energy left to battle! Please rest or visit the shop.");
      this.battleEnded = true;
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

    this.http.post<BattleResponse>(`${environment.apiUrl}/battle/turn`, req)
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
        },
        error: (err) => {
          this.battleLog.push("ERROR: " + (err.error?.message || err.statusText));
          this.isLoading = false;
        }
      });
  }
}
