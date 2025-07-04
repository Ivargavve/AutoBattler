import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { User } from './services/user';
import { Character } from './services/character';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { LoadingSpinnerComponent } from './components/loading-component/loading-component';
import {
  faHome,
  faFistRaised,
  faBoxOpen,
  faStore,
  faUsers,
  faTrophy,
  faBookOpen,
  faUserSecret
} from '@fortawesome/free-solid-svg-icons';
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
  public loading = true; 

  faHome = faHome;
  faFistRaised = faFistRaised;
  faBoxOpen = faBoxOpen;
  faStore = faStore;
  faUsers = faUsers;
  faTrophy = faTrophy;
  faBookOpen = faBookOpen;
  faUserSecret = faUserSecret;

  constructor(public auth: AuthService, private router: Router) {
    this.user$ = this.auth.user$;
    this.character$ = this.user$.pipe(
      map(user => user?.character ?? null)
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

  getXpRequired(level: number): number {
    return 100;
  }

  getXpPercentage(xp: number, level: number): number {
    const required = this.getXpRequired(level);
    return Math.min((xp / required) * 100, 100);
  }

}
