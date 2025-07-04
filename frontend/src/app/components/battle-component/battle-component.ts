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

  constructor(private http: HttpClient, private authService: AuthService) {}

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

        this.enemy = { name: 'Goblin', hp: 20, maxHp: 20 };
        this.battleLog = [];
        
        // Visa "Start New Battle" som default innan strid påbörjas
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
      this.battleEnded = false;  // Viktigt: striden är pågående
      this.battleLog = ['A wild Goblin appears!']; // Reset loggen med startmeddelandet

      this.http.get<any>('http://localhost:5051/api/characters/me').subscribe({
        next: (playerData) => {
          this.player = {
            name: playerData.name,
            hp: playerData.currentHealth,
            maxHp: playerData.maxHealth
          };
          this.playerEnergy = playerData.currentEnergy;
          this.userLevel = playerData.level;
          this.userXp = playerData.experiencePoints;

          this.enemy = { name: 'Goblin', hp: 20, maxHp: 20 };
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
      action: 'attack'
    };

    this.http.post<BattleResponse>('http://localhost:5051/api/battle/turn', req)
    .subscribe({
      next: (res) => {
        this.player!.hp = res.playerHp;
        this.player!.maxHp = res.playerMaxHp;
        this.enemy!.hp = res.enemyHp;
        this.enemy!.maxHp = res.enemyMaxHp;
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
