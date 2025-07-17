import { Component } from '@angular/core';
import { CommonModule, TitleCasePipe } from '@angular/common';

@Component({
  selector: 'app-battle-hub',
  templateUrl: './battle-hub.html',
  styleUrls: ['./battle-hub.scss'],
  imports: [TitleCasePipe, CommonModule]
})
export class BattleHubComponent {
  activeTab: string = 'welcome';

  playerLevel = 3;

  monsters = [
    { name: 'Goblin Grunt', level: 1, desc: 'Small, angry and green.' },
    { name: 'Zombie Accountant', level: 2, desc: 'Calculates your doom (slowly).' },
    { name: 'L33t Hacker Kid', level: 4, desc: 'He hacks you IRL.' },
    { name: 'Shadow Wolf', level: 6, desc: 'Fast and deadly in the dark.' }
  ];

  tips = [
    "💡 Tip: Monster fights reward XP and loot! Try different enemies for new challenges."
  ];
}
