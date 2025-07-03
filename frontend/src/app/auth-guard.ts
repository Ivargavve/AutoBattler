import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    console.log('AuthGuard activated!');
    console.log('Navigating to:', state.url);

    // Undantag för privacy
    if (state.url === '/privacy') {
      console.log('Access to /privacy allowed WITHOUT login.');
      return true;
    }

    const loggedIn = !!localStorage.getItem('token');
    console.log('Logged in:', loggedIn);

    if (!loggedIn) {
      console.log('Not logged in, redirecting to /login');
      this.router.navigate(['/login']); 
      return false;
    }

    console.log('Access granted.');
    return true;
  }
}
