import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { User } from './services/user';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterModule,
    CommonModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  public currentTheme: 'light' | 'dark' = 'light';
  public user$: Observable<User | null>;

  constructor(public auth: AuthService, private router: Router) {
    this.user$ = this.auth.user$;
  }

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('theme') as 'light' | 'dark';
    this.currentTheme = savedTheme || 'dark';
    this.applyTheme();

    if (this.auth.isLoggedIn) {
      this.auth.getProfile().subscribe({
        next: (profile) => {
          this.auth.setUser(profile);
          this.router.navigate(['/home']);
        },
        error: (err) => {
          console.error('Kunde inte hämta användarinfo', err);
          this.auth.logout();
        }
      });
    } else {
      this.router.navigate(['/login']);
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

  getXpRequired(level: number): number {
    return 100;
  }

  getXpPercentage(xp: number, level: number): number {
    const required = this.getXpRequired(level);
    return Math.min((xp / required) * 100, 100);
  }
}
