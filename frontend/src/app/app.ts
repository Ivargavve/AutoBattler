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
  public character$: Observable<Character | null>; // <- OBS! Nu från service
  public rechargeTimer$: Observable<string | null>;

  public loading = true;

  lastRechargeTime: Date | null = null;
  rechargeIntervalMinutes = 2;
  private rechargeCalled = false;
  icons = ICONS;

  constructor(public auth: AuthService, private router: Router) {
    this.user$ = this.auth.user$;

    // ÄNDRAT HÄR: Nu direkt från authService
    this.character$ = this.auth.character$;

    this.rechargeTimer$ = this.character$.pipe(
      switchMap(char => {
        if (!char || typeof char.nextTickInSeconds !== 'number') return of('00:00');

        if (
          char.currentEnergy === char.maxEnergy &&
          char.currentHealth === char.maxHealth
        ) {
          return of('Full!');
        }

        return timer(0, 1000).pipe(
          map(tickNum => {
            const tick = Math.max(0, (char.nextTickInSeconds ?? 0) - tickNum);
            const mins = Math.floor(tick / 60);
            const secs = tick % 60;
            return { mins, secs, tick };
          }),
          tap(({ tick }) => {
            if (tick === 0 && !this.rechargeCalled) {
              this.rechargeCalled = true;
              this.auth.rechargeCharacter()
                .then(character => {
                  // characterSubject.next() sker i service, inget mer krävs här!
                  // Kan slopa setUser-logik (sker i loadUserWithCharacter ändå)
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

    if (this.auth.isLoggedIn) {
      try {
        await this.auth.rechargeCharacter();
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
