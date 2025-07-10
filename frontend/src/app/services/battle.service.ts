import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class BattleService {
  inBattle$ = new BehaviorSubject<boolean>(false);

  constructor() {
    // Läs från localStorage när appen startar
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

  // För att “nollställa” allting om man loggar ut eller annat:
  clearBattle() {
    this.inBattle$.next(false);
    localStorage.setItem('inBattle', 'false');
  }
}
