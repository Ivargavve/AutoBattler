import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, lastValueFrom } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { User } from '../services/user';
import { Character } from '../services/character';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
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
    return this.http.get<User>(`${environment.apiUrl}/users/me`);
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

  getCharacter() {
    return this.http.get<Character>(`${environment.apiUrl}/characters/me`);
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
    return lastValueFrom(
      this.http.put<Character>(`${environment.apiUrl}/characters/recharge`, {})
    );
  }

}
