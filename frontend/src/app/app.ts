import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterModule, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import * as bootstrap from 'bootstrap';

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
  protected title = 'book-tracker';
  public currentTheme: 'light' | 'dark' = 'light';

  constructor(public auth: AuthService, private router: Router) {}

  ngOnInit(): void { 
    const savedTheme = localStorage.getItem('theme') as 'light' | 'dark'; // Retrieve saved theme from localStorage
    this.currentTheme = savedTheme || 'dark';
    this.applyTheme();
  }

  applyTheme(): void {
    document.body.classList.remove('light', 'dark'); 
    document.body.classList.add(this.currentTheme); // Apply the current theme to the body
  }

  get isLoggedIn(): boolean {
    return this.auth.isLoggedIn; 
  }

  logout(): void {
    this.auth.logout(); 
  }

}
