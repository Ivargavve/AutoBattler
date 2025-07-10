import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class BattleService {
  inBattle$ = new BehaviorSubject<boolean>(false);

  constructor() {
    const saved = localStorage.getItem('inBattle');
    if (saved === 'true') {
      this.inBattle$.next(true);
    } else {
      this.inBattle$.next(false);
    }
  }

  setInBattle(value: boolean) {
    this.inBattle$.next(value);
    localStorage.setItem('inBattle', value ? 'true' : 'false');
  }

  clearBattle() {
    this.inBattle$.next(false);
    localStorage.setItem('inBattle', 'false');
    this.clearBattleState();
  }

  saveBattleState(state: any) {
    localStorage.setItem('battleState', JSON.stringify(state));
  }

  loadBattleState(): any | null {
    const saved = localStorage.getItem('battleState');
    if (saved) {
      try {
        return JSON.parse(saved);
      } catch {
        this.clearBattleState();
      }
    }
    return null;
  }

  clearBattleState() {
    localStorage.removeItem('battleState');
  }
}

