import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    
    if (state.url === '/privacy') {
      return true;
    }

    const loggedIn = !!localStorage.getItem('token');

    if (!loggedIn) {
      this.router.navigate(['/login']); 
      return false;
    }
    return true;
  }
}
