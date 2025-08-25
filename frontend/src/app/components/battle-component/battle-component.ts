import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Fighter, BattleLogEntry, PlayerAttack } from '../../services/battle-interfaces';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';
import { Subscription, firstValueFrom } from 'rxjs';
import { Router } from '@angular/router';
import { BattleService } from '../../services/battle.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-battle',
  standalone: true,
  imports: [ CommonModule ],
  templateUrl: './battle-component.html',
  styleUrl: './battle-component.scss'
})
export class BattleComponent implements OnInit, AfterViewInit, OnDestroy {
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
  attacks: PlayerAttack[] = [];
  showNextButton = false;
  hoveredAttackDescription: string = '';
  selectedAttackDescription: string = '';
  lastShownDescription: string = '';

  // Player status effects
  isPlayerBlocking: boolean = false;
  isPlayerEvading: boolean = false; 
  isPlayerHealing: boolean = false;
  isPlayerPoisoned: boolean = false;
  playerCritBonus: number = 0;
  playerCritBonusTurns: number = 0;

  // Enemy status effects
  isEnemyBlocking: boolean = false;
  isEnemyHealing: boolean = false;
  isEnemyPoisoned: boolean = false;
  // Turn counters for status effects
  // Player turn counters
  playerBlockTurnsLeft: number = 0;
  playerHealTurnsLeft: number = 0;
  playerPoisonTurnsLeft: number = 0;
  // Enemy turn counters
  enemyBlockTurnsLeft: number = 0;
  enemyHealTurnsLeft: number = 0;
  enemyPoisonTurnsLeft: number = 0;

  private characterSub!: Subscription;

  @ViewChild('battleLogContainer') battleLogContainer!: ElementRef;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private router: Router,
    private battleService: BattleService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.characterSub = this.authService.character$.subscribe(character => {
      if (character) {
        this.player = {
          id: character.id,
          name: character.name,
          hp: character.currentHealth,
          maxHp: character.maxHealth
        };
        this.playerEnergy = character.currentEnergy;
        this.userLevel = character.level;
        this.userXp = character.experiencePoints;

        this.attacks = (character.attacks ?? (
          character.attacksJson ? JSON.parse(character.attacksJson) : []
        )).map((atk: any) => ({
          id: atk.Id,
          name: atk.Name,
          type: atk.Type,
          damageType: atk.DamageType,
          baseDamage: atk.BaseDamage,
          maxCharges: atk.MaxCharges,
          currentCharges: atk.CurrentCharges,
          scaling: atk.Scaling,
          requiredStats: atk.RequiredStats,
          allowedClasses: atk.AllowedClasses,
          description: atk.Description,
        }));

      } else {
        this.player = null;
        this.playerEnergy = 0;
        this.userLevel = null;
        this.userXp = null;
        this.attacks = [];
      }
    });

    if (this.attacks && this.attacks.length > 0) {
      this.lastShownDescription = this.getFullAttackDescription(this.attacks[0]);
    } else {
      this.lastShownDescription = '';
    }

    this.route.queryParams.subscribe(params => {
      this.enemyName = params['enemy'] || null;
    });

    const loadedState = this.battleService.loadBattleState();
    if (loadedState) {
      this.player = loadedState.player;
      this.enemy = loadedState.enemy;
      this.battleLog = loadedState.battleLog || [];
      this.battleEnded = loadedState.battleEnded;
      this.enemyName = loadedState.enemyName;
      this.userLevel = loadedState.userLevel;
      this.userXp = loadedState.userXp;
      this.playerEnergy = loadedState.playerEnergy;
      this.showNextButton = loadedState.showNextButton || false;
      this.isPlayerBlocking = loadedState.isPlayerBlocking || false;
      this.isPlayerEvading = loadedState.isPlayerEvading || false;
      this.isPlayerHealing = loadedState.isPlayerHealing || false;
      this.isPlayerPoisoned = loadedState.isPlayerPoisoned || false;
      this.isEnemyPoisoned = loadedState.isEnemyPoisoned || false;
      this.playerCritBonus = loadedState.playerCritBonus || 0;
      this.playerCritBonusTurns = loadedState.playerCritBonusTurns || 0;
    } else {
      this.battleLog = [];
      this.battleEnded = true;
      this.startNewBattle();
    }
  }

  ngAfterViewInit(): void {
    this.scrollToBottom();
  }

  ngOnDestroy(): void {
    if (this.characterSub) {
      this.characterSub.unsubscribe();
    }
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

  startNewBattle(): void {
    this.battleService.setInBattle(true);
    if (this.playerEnergy > 0) {
      this.isLoading = true;
      this.battleEnded = false;
      this.enemy = null;
      this.isPlayerBlocking = false; 
      this.isPlayerEvading = false;
      this.isEnemyPoisoned = false;
      this.playerCritBonus = 0;
      this.playerCritBonusTurns = 0;
      this.battleLog = [{ message: "Starting new battle...", type: "start" }];
      this.scrollToBottom();

      const enemyQuery = this.enemyName ? `?enemyName=${encodeURIComponent(this.enemyName)}` : '';
      this.http.get<any>(`${environment.apiUrl}/battle/encounter${enemyQuery}`).subscribe({
        next: (enemyData) => {
          this.enemyName = enemyData.enemyName;
          this.enemy = {
            id: enemyData.enemyId ?? 0,
            name: enemyData.enemyName,
            hp: enemyData.enemyHp,
            maxHp: enemyData.enemyMaxHp
          };
          this.battleLog.push({ message: `You encounter a wild ${this.enemyName}! Prepare for battle!`, type: "encounter" });
          this.isLoading = false;
          this.scrollToBottom();
          this.saveBattleState();
        },
        error: (err) => {
          this.battleLog = [{ message: "ERROR: " + (err.error?.message || err.statusText), type: "error" }];
          this.isLoading = false;
          this.scrollToBottom();
          this.saveBattleState();
        }
      });

    } else {
      this.battleLog.push({ message: "You have no energy left to battle! Please rest or visit the shop.", type: "info" });
      this.battleEnded = true;
      this.scrollToBottom();
      this.saveBattleState();
    }
  }

  useAttack(attack: PlayerAttack): void {
    if (!this.player || !this.enemy || this.battleEnded || this.playerEnergy <= 0 || attack.currentCharges <= 0) return;

    this.isLoading = true;

    const req = {
      playerId: this.player.id,
      enemyHp: this.enemy.hp,
      enemyName: this.enemy.name,
      action: 'attack',
      attackId: attack.id,
      playerCritBonus: this.playerCritBonus,
      playerCritBonusTurns: this.playerCritBonusTurns
    };

    this.http.post<any>(`${environment.apiUrl}/battle/turn`, req)
      .subscribe({
        next: async (res) => {
          this.player!.hp = res.playerHp;
          this.player!.maxHp = res.playerMaxHp;
          this.enemy!.hp = res.enemyHp;
          this.enemy!.maxHp = res.enemyMaxHp;
          this.enemy!.name = res.enemyName;
          this.enemyName = res.enemyName;
          this.isPlayerBlocking = res.isPlayerBlocking || false;
          this.isPlayerEvading = res.isPlayerEvading || false; 
          this.isPlayerHealing = res.isPlayerHealing || false;
          this.isPlayerPoisoned = res.isPlayerPoisoned || false;
          this.isEnemyPoisoned = res.isEnemyPoisoned || false;
          this.playerCritBonus = res.playerCritBonus ?? 0;
          this.playerCritBonusTurns = res.playerCritBonusTurns ?? 0;
          this.battleLog.push(...res.battleLog);
          this.battleEnded = res.battleEnded;

          if (res.battleEnded && this.player!.hp > 0) {
            this.gainedXp = res.gainedXp ?? null;
            this.onBattleEnd();
          } else if (res.battleEnded && this.player!.hp <= 0) {
            this.isLoading = true;
            try {
              await firstValueFrom(this.http.delete(`${environment.apiUrl}/characters`));
            } catch {}
            this.isLoading = false;
            this.gainedXp = null;
            this.onBattleEnd();
          } else {
            this.gainedXp = null;
          }
          this.authService.loadUserWithCharacter();
          this.isLoading = false;
          this.scrollToBottom();
          this.saveBattleState();
        },
        error: (err) => {
          this.battleLog.push({ message: "ERROR: " + (err.error?.message || err.statusText), type: "error" });
          this.isLoading = false;
          this.scrollToBottom();
          this.saveBattleState();
        }
      });
  }

  private resetBattleAndNavigate(): void {
    try {
      this.saveBattleState();              
      this.battleService.setInBattle(false);
      this.battleService.clearBattleState();
      this.player = null;
      this.enemy = null;
      this.battleLog = [];
      this.battleEnded = true;
      this.gainedXp = null;
      this.enemyName = null;
      this.userLevel = null;
      this.userXp = null;
      this.playerEnergy = 0;
      this.attacks = [];
      this.isPlayerBlocking = false;
      this.isPlayerEvading = false;
      this.isPlayerHealing = false;
      this.isPlayerPoisoned = false;
      this.isEnemyPoisoned = false;
      this.playerCritBonus = 0;
      this.playerCritBonusTurns = 0;
    } catch (err) {} finally {
      this.isLoading = false;
      this.showNextButton = false;
      this.router.navigate(['/battle-hub']);
    }
  }

  onBattleEnd() {
    this.showNextButton = true;
    this.saveBattleState();
  }

  runFromBattle() {
    if (this.playerEnergy > 0) {
      this.isLoading = true;
      this.authService.useEnergy(1).then(() => {
        this.resetBattleAndNavigate();
      }).catch(() => {
        this.resetBattleAndNavigate();
      });
    } else {
      this.resetBattleAndNavigate();
    }
  }

  nextBattle() {
    this.isLoading = true;
    this.resetBattleAndNavigate();
  }

  setHoveredAttack(attack: PlayerAttack) {
    this.hoveredAttackDescription = this.getFullAttackDescription(attack);
    this.lastShownDescription = this.hoveredAttackDescription;
  }

  clearHoveredAttack() {
    this.hoveredAttackDescription = '';
  }

  selectAttack(attack: PlayerAttack) {
    this.lastShownDescription = attack.description;
    if (attack.id !== -1 && attack.currentCharges > 0 && !this.isLoading && !this.battleEnded) {
      this.useAttack(attack);
    }
  }

  getFullAttackDescription(attack: PlayerAttack): string {
    if (!attack || attack.id === -1) return '';
    let name = attack.name;
    let desc = attack.description ? attack.description : '';
    return name + ':::' + desc; 
  }

  getAttackDescription(): string {
    return this.hoveredAttackDescription || this.lastShownDescription || '';
  }

  getAttackTypeLabel(type: string): string {
    switch (type) {
      case 'physical': return 'Physical';
      case 'magic': return 'Magic';
      case 'heal': return 'Heal';
      case 'buff': return 'Buff';
      case 'block': return 'Block';
      default: return type ? type.charAt(0).toUpperCase() + type.slice(1) : '';
    }
  }

  getChargeClass(attack: PlayerAttack): string {
    if (attack.id === -1) return 'charge-empty';
    if (attack.currentCharges === 0) return 'charge-empty';
    if (attack.currentCharges / attack.maxCharges > 0.5) return 'charge-high';
    if (attack.currentCharges / attack.maxCharges > 0.25) return 'charge-mid';
    return 'charge-low';
  }
  
  saveBattleState() {
    this.battleService.saveBattleState({
      player: this.player,
      enemy: this.enemy,
      battleLog: this.battleLog,
      battleEnded: this.battleEnded,
      enemyName: this.enemyName,
      userLevel: this.userLevel,
      userXp: this.userXp,
      playerEnergy: this.playerEnergy,
      showNextButton: this.showNextButton,
      isPlayerBlocking: this.isPlayerBlocking,
      isPlayerEvading: this.isPlayerEvading,
      isPlayerHealing: this.isPlayerHealing,
      isPlayerPoisoned: this.isPlayerPoisoned,
      isEnemyPoisoned: this.isEnemyPoisoned,
      playerCritBonus: this.playerCritBonus,
      playerCritBonusTurns: this.playerCritBonusTurns
    });
  }

  get displayAttacks(): PlayerAttack[] {
    const filled = [...this.attacks];
    while (filled.length < 4) {
      filled.push({
        id: -1,
        name: 'Empty',
        type: '',
        damageType: '',
        baseDamage: 0,
        maxCharges: 0,
        currentCharges: 0,
        scaling: {},
        requiredStats: {},
        allowedClasses: [],
        description: 'No attack equipped'
      });
    }
    return filled.slice(0, 4);
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
