import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from './services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private router: Router, private auth: AuthService) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    
    if (state.url === '/privacy') {
      return true;
    }

    const loggedIn = !!localStorage.getItem('token');

    if (!loggedIn) {
      this.router.navigate(['/login']); 
      return false;
    }

    if (state.url === '/battle') {
      const user = this.auth.user;

      if (!user || !user.character) {
        this.router.navigate(['/create-character']);
        return false;
      }
    }
    return true;
  }
}
