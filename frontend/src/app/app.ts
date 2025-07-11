import { Component, OnInit} from '@angular/core';
import { RouterOutlet, RouterModule, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { User } from './services/user';
import { Character } from './services/character';
import { Observable, timer, of, firstValueFrom, combineLatest } from 'rxjs';
import { map, switchMap, startWith, tap, delay, filter } from 'rxjs/operators';
import { LoadingSpinnerComponent } from './components/loading-component/loading-component';
import { ICONS } from './icons'; 
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { FriendsListComponent } from './components/friends-list/friends-list';
import { BattleService } from './services/battle.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterModule, 
    CommonModule,
    LoadingSpinnerComponent,
    FontAwesomeModule,
    FriendsListComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  public currentTheme: 'light' | 'dark' = 'light';
  public user$: Observable<User | null>;
  public character$: Observable<Character | null>;
  public rechargeTimer$: Observable<string | null>;
  showReturnToBattle$!: Observable<boolean>;
  public currentRoute$!: Observable<string>;
  public loading = true;
  lastRechargeTime: Date | null = null;
  rechargeIntervalMinutes = 2;
  private rechargeCalled = false;
  icons = ICONS;

  constructor(public auth: AuthService, private router: Router, private battleService: BattleService) {
    this.user$ = this.auth.user$;
    this.character$ = this.auth.character$;

    this.showReturnToBattle$ = combineLatest([
      this.battleService.inBattle$,
      this.router.events.pipe(
        filter(e => e instanceof NavigationEnd),
        map(() => this.router.url),
        startWith(this.router.url) 
      )
    ]).pipe(
      map(([inBattle, url]) => {
        const onBattlePage = /^\/battle($|\?)/.test(url);
        const onLoginPage = url === '/login';
        return inBattle && !onBattlePage && !onLoginPage;
      }),
      delay(0)
    );

    this.rechargeTimer$ = combineLatest([
      this.character$,
      this.battleService.inBattle$
    ]).pipe(
      switchMap(([char, inBattle]) => {
        if (!char || typeof char.nextTickInSeconds !== 'number') return of('00:00');
        if (char.currentEnergy === char.maxEnergy && char.currentHealth === char.maxHealth) {
          return of('Full!');
        }
        if (inBattle) return of('(Battle)'); 
        let tickNum = 0;
        return timer(0, 1000).pipe(
          map(() => {
            if (!inBattle) tickNum++;
            return tickNum;
          }),
          map(tickNumValue => {
            const tick = Math.max(0, (char.nextTickInSeconds ?? 0) - tickNumValue);
            const mins = Math.floor(tick / 60);
            const secs = tick % 60;
            return { mins, secs, tick };
          }),
          tap(({ tick }) => {
            if (tick === 0 && !this.rechargeCalled) {
              this.rechargeCalled = true;
              this.auth.rechargeCharacter()
                .then(character => {
                  this.auth.loadUserWithCharacter();
                })
                .finally(() => {
                  setTimeout(() => this.rechargeCalled = false, 2000);
                });
            }
          }),
          map(({ mins, secs }) => `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`),
          startWith('00:00')
        );
      })
    );
  }

async ngOnInit(): Promise<void> {
  this.loading = true;
  const savedTheme = localStorage.getItem('theme') as 'light' | 'dark';
  this.currentTheme = savedTheme || 'dark';
  this.applyTheme();

  const currentUrl = this.router.url;

  if (this.auth.isLoggedIn && currentUrl === '/login') {
    this.router.navigate(['/home']);
    this.loading = false;
    return;
  }
  if (!this.auth.isLoggedIn) {
    this.loading = false;
    return;
  }

  try {
    const user: User | null = await firstValueFrom(this.auth.getProfile());
    if (!user) {
      this.auth.logout();
      this.router.navigate(['/login']);
      this.loading = false;
      return;
    }
    this.auth.setUser(user);
    let character: Character | null = null;
    try {
      character = await firstValueFrom(this.auth.getCharacter());
      this.auth['characterSubject'].next(character); 
    } catch {
      character = null;
      this.auth['characterSubject'].next(null);
    }
    this.auth.rechargeCharacter();
    this.auth.loadUserWithCharacter();
  } catch (error) {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
  this.loading = false;
}

  get showPanels$(): Observable<boolean> {
    return this.auth.user$.pipe(
      map(user => !!user && !user.needsUsernameSetup)
    );
  }

  applyTheme(): void {
    document.body.classList.remove('light', 'dark');
    document.body.classList.add(this.currentTheme);
  }

  get isLoggedIn(): boolean {
    return this.auth.isLoggedIn;
  }

  logout(): void {
    this.auth.logout();
  }

  getXpPercentage(xp: number): number {
    return Math.max(0, Math.min((xp / 100) * 100, 100));
  }
}
