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
    path: 'create-character', 
    loadComponent: () => import('./components/create-character/create-character').then(m => m.CreateCharacterComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'privacy', 
    loadComponent: () => import('./components/privacy-component/privacy-component').then(m => m.PrivacyComponent),
  },
  { 
    path: 'home', 
    loadComponent: () => import('./components/home-component/home-component').then(m => m.HomeComponent),
    canActivate: [AuthGuard]
  }
];
