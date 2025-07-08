import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-battle-planner',
  templateUrl: './battle-planner.html',
  styleUrls: ['./battle-planner.scss']
})
export class BattlePlannerComponent {
  constructor(private router: Router) {}

  fight(enemyName: string) {
    this.router.navigate(['/battle'], {
      queryParams: { enemy: enemyName }
    });
  }
}
