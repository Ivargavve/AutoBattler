import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, lastValueFrom, firstValueFrom } from 'rxjs';
import { tap, map, take } from 'rxjs/operators';
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

  private normalizeCharacterIcon<T extends { profileIconUrl?: string }>(obj: T | null | undefined): T | null | undefined {
    if (!obj) return obj;
    const url = obj.profileIconUrl || '';
    if (/^char\d+\.jpeg$/i.test(url)) {
      obj.profileIconUrl = `assets/characters/${url}` as any;
    } else if (/^assets\/char\d+\.jpeg$/i.test(url)) {
      obj.profileIconUrl = url.replace(/^assets\//i, 'assets/characters/') as any;
    }
    return obj;
  }

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
    const normalizedUser: any = { ...user };
    if (normalizedUser.character) {
      this.normalizeCharacterIcon(normalizedUser.character);
    }
    this.userSubject.next(normalizedUser);

    if (user.character) {
      this.characterSubject.next(this.normalizeCharacterIcon(user.character) as any);
    }
  }
  loadUserWithCharacter(): Promise<void> {
    return lastValueFrom(this.getProfile()).then(profile => {
      return lastValueFrom(this.getCharacter())
        .then(character => {
          this.characterSubject.next(this.normalizeCharacterIcon(character) as any);
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
      this.characterSubject.next(this.normalizeCharacterIcon(character) as any);
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

  updateMissionRewardData(characterData?: any, userData?: any): void {
    // Update character data if provided
    if (characterData) {
      const normalizedCharacter = this.normalizeCharacterIcon(characterData);
      this.characterSubject.next(normalizedCharacter as any);
      
      // Also update the user's character data
      const currentUser = this.user;
      if (currentUser) {
        const updatedUser = { ...currentUser, character: normalizedCharacter };
        this.setUser(updatedUser);
      }
    }
    
    // Update user data if provided (for user rewards like credits)
    if (userData) {
      const currentUser = this.user;
      if (currentUser) {
        const updatedUser = { ...currentUser, ...userData };
        this.setUser(updatedUser);
      }
    }
  }

  equipItem(slot: string, itemId: number) {
    return this.http.post<{ id: number; equipmentJson: string }>(
      `${environment.apiUrl}/characters/equipment/equip`,
      { slot, itemId }
    ).pipe(
      tap(() => {
        // Refresh character after change
        this.loadUserWithCharacter();
      })
    );
  }

  unequipItem(slot: string) {
    return this.http.post<{ message: string; equipmentJson: string }>(
      `${environment.apiUrl}/characters/equipment/unequip`,
      { slot }
    ).pipe(
      tap(() => {
        this.loadUserWithCharacter();
      })
    );
  }

  equipAbilities(attackIds: number[]) {
    return this.http.post<{ attacksJson: string }>(
      `${environment.apiUrl}/characters/attacks/equip`,
      { attackIds }
    ).pipe(
      tap(() => {
        this.loadUserWithCharacter();
      })
    );
  }

  getCurrentCharacter(): Character | null {
    let currentCharacter: Character | null = null;
    this.character$.pipe(take(1)).subscribe(char => currentCharacter = char);
    return currentCharacter;
  }

}
