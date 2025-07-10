import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from './services/auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router, private auth: AuthService) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> | boolean {
    const loggedIn = !!localStorage.getItem('token');
    if (!loggedIn) {
      this.router.navigate(['/login']);
      return false;
    }
    if (!this.auth.user) {
      return this.auth.loadUserWithCharacter().then(() => {
        const user = this.auth.user;
        if (!user) {
          this.router.navigate(['/login']);
          return false;
        }
        if (state.url === '/battle' && !user.character) {
          this.router.navigate(['/create-character']);
          return false;
        }
        return true;
      }).catch(() => {
        this.router.navigate(['/login']);
        return false;
      });
    }
    if (state.url === '/battle' && !this.auth.user?.character) {
      this.router.navigate(['/create-character']);
      return false;
    }
    return true;
  }
}
