import { Routes } from '@angular/router';
import { AuthGuard } from './auth-guard'; 

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { 
    path: 'login', 
    loadComponent: () => import('./components/login-form/login-form').then(m => m.LoginForm) 
  },
  { 
    path: 'username-form',
    loadComponent: () => import('./components/username-form/username-form').then(m => m.UsernameSetupComponent),
    canActivate: [AuthGuard] 
  },
  { 
    path: 'profile', 
    loadComponent: () => import('./components/profile-component/profile-component').then(m => m.ProfileComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'home', 
    loadComponent: () => import('./components/home-component/home-component').then(m => m.HomeComponent),
    canActivate: [AuthGuard]
  }
];
