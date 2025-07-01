import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private googleUrl = `${environment.apiUrl}/googleauth`;

  constructor(private http: HttpClient, private router: Router) {}

  googleLogin(credential: string) {
    return this.http.post<{ token: string }>(`${this.googleUrl}/login`, { credential })
      .pipe(tap(response => this.saveToken(response.token)));
  }

  private saveToken(token: string) {
    localStorage.setItem('token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  get isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
  getProfile() {
    const token = this.getToken();
    const headers = token
      ? new HttpHeaders().set('Authorization', `Bearer ${token}`)
      : undefined;

    return this.http.get(`${environment.apiUrl}/users/me`, { headers });
  }
}
