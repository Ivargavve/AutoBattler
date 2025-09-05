import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, lastValueFrom, firstValueFrom } from 'rxjs';
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

  private characterSubject = new BehaviorSubject<Character | null>(null);
  public character$ = this.characterSubject.asObservable();

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
    this.characterSubject.next(null); 
    this.router.navigate(['/login']);
  }

  getProfile() {
    return this.http.get<User>(`${environment.apiUrl}/users/me`);
  }

  getProfileByUsername(username: string) {
    return this.http.get<User>(`${environment.apiUrl}/users/${encodeURIComponent(username)}`);
  }

  getCharacter() {
    return this.http.get<Character>(`${environment.apiUrl}/characters/me`);
  }

  getCharacterByUsername(username: string) {
    return this.http.get<Character>(`${environment.apiUrl}/characters/${encodeURIComponent(username)}`);
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

    if (user.character) {
      this.characterSubject.next(user.character);
    }
  }
  loadUserWithCharacter(): Promise<void> {
    return lastValueFrom(this.getProfile()).then(profile => {
      return lastValueFrom(this.getCharacter())
        .then(character => {
          this.characterSubject.next(character);
          const newUser = { ...profile, character };
          this.setUser(newUser);
        })
        .catch(() => {
          this.characterSubject.next(null);
          this.setUser({ ...profile, character: undefined });
        });
    });
  }
  rechargeCharacter(): Promise<void> {
    return lastValueFrom(
      this.http.put<Character>(`${environment.apiUrl}/characters/recharge`, {})
    ).then(character => {
      this.characterSubject.next(character);
    });
  }
  useEnergy(amount = 1): Promise<number> {
    return lastValueFrom(
      this.http.post<{ currentEnergy: number }>(
        `${environment.apiUrl}/characters/use-energy`, { amount }
      )
    ).then(res => {
      return this.loadUserWithCharacter().then(() => res.currentEnergy);
    });
  }
  updateCharacterStats(payload: {
    attack: number;
    defense: number;
    agility: number;
    magic: number;
    speed: number;
    maxHealth: number;
  }) {
    return this.http.patch<Character>(
      `${environment.apiUrl}/characters/stats`,
      payload
    ).pipe(
      tap(updated => {
        this.characterSubject.next(updated);
      })
    );
  }

}
