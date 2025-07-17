import { Routes } from '@angular/router';
import { AuthGuard } from './auth-guard'; 
import { NoAuthGuard } from './no-auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { 
    path: 'login', 
    loadComponent: () => import('./components/login-form/login-form').then(m => m.LoginForm),
    canActivate: [NoAuthGuard]  
  },
  { 
    path: 'username-form',
    loadComponent: () => import('./components/username-form/username-form').then(m => m.UsernameSetupComponent),
    canActivate: [AuthGuard] 
  },
  { 
    path: 'profile/:username', 
    loadComponent: () => import('./components/profile-component/profile-component').then(m => m.ProfileComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'profile/id/:id', 
    loadComponent: () => import('./components/profile-component/profile-component').then(m => m.ProfileComponent),
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
  },
  { 
    path: 'battle-planner', 
    loadComponent: () => import('./components/battle-planner/battle-planner').then(m => m.BattlePlannerComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'battle-hub', 
    loadComponent: () => import('./components/battle-hub/battle-hub').then(m => m.BattleHubComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'battle', 
    loadComponent: () => import('./components/battle-component/battle-component').then(m => m.BattleComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'vault', 
    loadComponent: () => import('./components/vault-component/vault-component').then(m => m.VaultComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'bazaar', 
    loadComponent: () => import('./components/bazaar-component/bazaar-component').then(m => m.BazaarComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'social', 
    loadComponent: () => import('./components/social-component/social-component').then(m => m.SocialComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'hall-of-legends', 
    loadComponent: () => import('./components/hall-of-legends-component/hall-of-legends-component').then(m => m.HallOfLegendsComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'tales', 
    loadComponent: () => import('./components/tales-component/tales-component').then(m => m.TalesComponent),
    canActivate: [AuthGuard]
  },
  { 
    path: 'hero', 
    loadComponent: () => import('./components/hero-component/hero-component').then(m => m.HeroComponent),
    canActivate: [AuthGuard]
  }
];
