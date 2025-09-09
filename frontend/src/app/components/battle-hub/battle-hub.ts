import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, TitleCasePipe } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service'; 
import { Character } from '../../services/character';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { TitleService } from '../../services/title.service';

@Component({
  selector: 'app-battle-hub',
  templateUrl: './battle-hub.html',
  styleUrls: ['./battle-hub.scss'],
  imports: [TitleCasePipe, CommonModule]
})
export class BattleHubComponent implements OnInit, OnDestroy {
  activeTab: string = 'monsters';
  character: Character | null = null;
  monsters: any[] = [];
  selectedMonster: any = null;

  constructor(
    private router: Router,
    private authService: AuthService,
    private http: HttpClient,
    private titleService: TitleService
  ) {
    this.authService.character$.subscribe(char => {
      this.character = char;
      // Re-select first available monster when character changes
      this.selectFirstAvailableMonster();
    });

    this.http.get<any[]>(`${environment.apiUrl}/enemies`).subscribe(enemies => {
      this.monsters = enemies
        .map(e => ({
          name: e.name,
          level: e.level ?? 1,
          desc: e.description || e.desc || "",
          type: e.type || "unknown",
          maxHp: e.maxHp,
          attack: e.attack,
          defense: e.defense,
          xp: e.xp,
          critChance: e.critChance,
          imageUrl: e.imageUrl || "assets/monsters/skeleton.jpg"
        }))
        .sort((a, b) => a.level - b.level);
      
      // Select the first available monster by default
      this.selectFirstAvailableMonster();
    });
  }

  ngOnInit() {
    this.titleService.setTitle('Battle Hub');
  }

  ngOnDestroy() {
    this.titleService.setBaseTitle();
  }

  selectMonster(monster: any) {
    this.selectedMonster = monster;
  }

  selectFirstAvailableMonster() {
    if (this.monsters.length > 0) {
      // Find the first monster that the character can fight
      const availableMonster = this.monsters.find(monster => 
        !this.character || this.character.level >= monster.level
      );
      
      // If no available monster found, select the first one anyway (will be locked)
      this.selectedMonster = availableMonster || this.monsters[0];
    }
  }

  fight(enemyName: string) {
    if (this.character && this.character.currentEnergy > 0) {
      this.router.navigate(['/battle'], {
        queryParams: { enemy: enemyName }
      });
    }
  }

  onImageError(event: any) {
    console.log('Monster image failed to load:', event.target.src);
    // Set fallback image
    event.target.src = 'assets/monsters/skeleton.jpg';
  }
}
