import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { Character } from '../../services/character';
import { PlayerAttack } from '../../services/battle-interfaces';
import { Observable, Subscription, of } from 'rxjs';

@Component({
  selector: 'app-vault-component',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vault-component.html',
  styleUrls: ['./vault-component.scss'],
})
export class VaultComponent implements OnInit {
  character: Character | null = null;
  characterSub!: Subscription;
  inventory: any[] = [];
  equippedSlots: any[] = [];
  attacks: PlayerAttack[] = [];

  defaultAvatar = '/assets/default-avatar.png';
  fallbackIcon = '/assets/fallback-item.png';

  totalItems = 0;
  uniqueGearCount = 0;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    this.characterSub = this.authService.character$.subscribe(character => {
      if (character) {
        this.character = character;
        try {
          this.inventory = JSON.parse(character.inventoryJson || '[]');
        } catch {
          this.inventory = [];
        }

        try {
          this.equippedSlots = JSON.parse(character.equipmentJson || '[]');
        } catch {
          this.equippedSlots = [];
        }

        if (character.attacks && character.attacks.length > 0) {
          this.attacks = character.attacks;
        } else if (character.attacksJson) {
          try {
            const rawAttacks = JSON.parse(character.attacksJson);
            this.attacks = rawAttacks.map((atk: any) => ({
              id: atk.Id ?? atk.id,
              name: atk.Name ?? atk.name,
              type: atk.Type ?? atk.type,
              damageType: atk.DamageType ?? atk.damageType,
              baseDamage: atk.BaseDamage ?? atk.baseDamage,
              maxCharges: atk.MaxCharges ?? atk.maxCharges,
              currentCharges: atk.CurrentCharges ?? atk.currentCharges,
              scaling: atk.Scaling ?? atk.scaling ?? {},
              requiredStats: atk.RequiredStats ?? atk.requiredStats ?? {},
              allowedClasses: atk.AllowedClasses ?? atk.allowedClasses ?? [],
              description: atk.Description ?? atk.description ?? '',
            }));
          } catch {
            this.attacks = [];
          }
        } else {
          this.attacks = [];
        }

        this.totalItems = this.inventory.reduce((sum, item) => sum + (item.quantity || 1), 0);
        this.uniqueGearCount = new Set(this.inventory.map(i => i.name)).size;
      } else {
        this.character = null;
        this.inventory = [];
        this.equippedSlots = [];
        this.attacks = [];
        this.totalItems = 0;
        this.uniqueGearCount = 0;
      }
    });
  }

  ngOnDestroy() {
    if (this.characterSub) this.characterSub.unsubscribe();
  }
}
