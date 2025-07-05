import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { User } from './services/user';
import { Character } from './services/character';
import { Observable, timer, of } from 'rxjs';
import { map, switchMap, startWith, tap } from 'rxjs/operators';
import { LoadingSpinnerComponent } from './components/loading-component/loading-component';
import { ICONS } from './icons'; 
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterModule,
    CommonModule,
    LoadingSpinnerComponent,
    FontAwesomeModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  public currentTheme: 'light' | 'dark' = 'light';
  public user$: Observable<User | null>;
  public character$: Observable<Character | null>;
  public rechargeTimer$: Observable<string | null>;

  public loading = true;

  lastRechargeTime: Date | null = null;
  rechargeIntervalMinutes = 2;
  private rechargeCalled = false;
  icons = ICONS;

  constructor(public auth: AuthService, private router: Router) {
    this.user$ = this.auth.user$;
    this.character$ = this.user$.pipe(
      map(user => user?.character ?? null)
    );

    this.rechargeTimer$ = this.character$.pipe(
      switchMap(char => {
        if (!char?.lastRechargeTime) return of('00:00');
        return timer(0, 1000).pipe(
          map(() => {
            const lastRecharge = char.lastRechargeTime;
            const last = (typeof lastRecharge === 'string' || lastRecharge instanceof Date)
              ? new Date(lastRecharge)
              : new Date();
            const now = new Date();
            const diffSec = Math.floor((now.getTime() - last.getTime()) / 1000);
            const intervalSec = this.rechargeIntervalMinutes * 60;
            const timeUntilNext = intervalSec - (diffSec % intervalSec);
            const mins = Math.floor(timeUntilNext / 60);
            const secs = timeUntilNext % 60;
            return { mins, secs, timerString: `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}` };
          }),
          tap(({ mins, secs }) => {
            if (mins === 0 && secs <= 1 && !this.rechargeCalled) {
              this.rechargeCalled = true;
              this.auth.rechargeCharacter()
                .then(character => {
                  const user = this.auth.user;
                  if (user) {
                    user.character = character;
                    this.auth.setUser(user);
                  }
                })
                .finally(() => {
                  setTimeout(() => this.rechargeCalled = false, 2000);
                });
            }
          }),
          map(({ timerString }) => timerString)
        );
      }),
      startWith('00:00')
    );
  }

  async ngOnInit(): Promise<void> {
    this.loading = true;
    const savedTheme = localStorage.getItem('theme') as 'light' | 'dark';
    this.currentTheme = savedTheme || 'dark';
    this.applyTheme();

    const currentUrl = this.router.url;

    if (this.auth.isLoggedIn) {
      try {
        await this.auth.loadUserWithCharacter();

        if (currentUrl === '/login' || currentUrl === '/') {
          this.router.navigate(['/home']);
        }
      } catch (error) {
        this.auth.logout();
        this.router.navigate(['/login']);
      }
    }
    this.loading = false;
  }

  setLastRechargeTimeFromCharacter(): void {
    const user = this.auth.user;
    if (user && user.character && user.character.lastRechargeTime) {
      this.lastRechargeTime = new Date(user.character.lastRechargeTime);
    } else {
      this.lastRechargeTime = null;
    }
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
