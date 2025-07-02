import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';

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
  public user: any = null;

  constructor(public auth: AuthService, private router: Router) {}

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('theme') as 'light' | 'dark';
    this.currentTheme = savedTheme || 'dark';
    this.applyTheme();

    if (this.isLoggedIn) {
      this.auth.getProfile().subscribe({
        next: (data) => {
          this.user = data;
        },
        error: (err) => {
          console.error('Kunde inte hämta användarinfo', err);
        }
      });
    }
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
    return 33;
  }

  getXpPercentage(xp: number, level: number): number {
    const required = this.getXpRequired(level);
    return Math.min((xp / required) * 100, 100);
  }
}
