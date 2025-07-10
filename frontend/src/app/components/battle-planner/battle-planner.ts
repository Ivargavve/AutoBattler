import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service'; // Anpassa path om nödvändigt
import { Character } from '../../services/character';

@Component({
  selector: 'app-battle-planner',
  templateUrl: './battle-planner.html',
  styleUrls: ['./battle-planner.scss']
})
export class BattlePlannerComponent {
  character: Character | null = null;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    this.authService.character$.subscribe(char => {
      this.character = char;
    });
  }

  fight(enemyName: string) {
    if (this.character && this.character.currentEnergy > 0) {
      this.router.navigate(['/battle'], {
        queryParams: { enemy: enemyName }
      });
    }
  }
}
