import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { User } from '../services/user';
import { map } from 'rxjs/operators';
import { Character } from '../services/character';
import { lastValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private googleUrl = `${environment.apiUrl}/googleauth`;

  private userSubject = new BehaviorSubject<User | null>(this.loadUserFromStorage());
  public user$ = this.userSubject.asObservable();

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

  public isLoggedIn$ = this.user$.pipe(
    map(user => !!user && !!this.getToken())
  );

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.userSubject.next(null);
    this.router.navigate(['/login']);
  }

  getProfile() {
    const token = this.getToken();
    const headers = token
      ? new HttpHeaders().set('Authorization', `Bearer ${token}`)
      : undefined;

    return this.http.get<User>(`${environment.apiUrl}/users/me`, { headers });
  }

  private loadUserFromStorage(): User | null {
    const raw = localStorage.getItem('user');
    return raw ? JSON.parse(raw) : null;
  }

  public get user(): User | null {
    return this.userSubject.value;
  }

  public setUser(user: User): void {
    localStorage.setItem('user', JSON.stringify(user));
    this.userSubject.next({ ...user });
  }

  getAuthHeaders(): { headers: HttpHeaders } {
    const token = this.getToken();
    const headers = token ? new HttpHeaders().set('Authorization', `Bearer ${token}`) : new HttpHeaders();
    return { headers };
  }

  getCharacter() {
    const token = this.getToken();
    const headers = token ? new HttpHeaders().set('Authorization', `Bearer ${token}`) : undefined;
    return this.http.get<Character>(`${environment.apiUrl}/characters/me`, { headers });
  }

  loadUserWithCharacter(): Promise<void> {
    return lastValueFrom(this.getProfile()).then(profile => {
      return lastValueFrom(this.getCharacter())
        .then(character => {
          const newUser = { ...profile, character };
          this.setUser(newUser);
        })
        .catch(() => {
          this.setUser({ ...profile, character: undefined });
          
        });
    });
  }
  rechargeCharacter(): Promise<Character> {
    const token = this.getToken();
    const headers = token ? new HttpHeaders().set('Authorization', `Bearer ${token}`) : undefined;
    return lastValueFrom(
      this.http.put<Character>(`${environment.apiUrl}/characters/recharge`, {}, { headers })
    );
  }
}
