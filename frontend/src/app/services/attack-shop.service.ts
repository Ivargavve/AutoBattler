import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AttackShopItem {
  id: number;
  name: string;
  type: string;
  damageType: string;
  baseDamage: number;
  maxCharges: number;
  scaling: { [key: string]: number };
  requiredStats: { [key: string]: number };
  allowedClasses: string[];
  description: string;
  healAmount: number;
  blockNextAttack: boolean;
  poison: boolean;
  evadeNextAttack: boolean;
  critChanceBonus: number;
  critBonusTurns: number;
  poisonDamagePerTurn: number;
  poisonDuration: number;
  price: number;
  canAfford: boolean;
  meetsRequirements: boolean;
  isAvailable: boolean;
}

export interface PurchaseResponse {
  message: string;
  remainingCredits: number;
  attack: any;
}

@Injectable({
  providedIn: 'root'
})
export class AttackShopService {
  private apiUrl = `${environment.apiUrl}/attack-shop`;

  constructor(private http: HttpClient) { }

  getAvailableAttacks(): Observable<AttackShopItem[]> {
    return this.http.get<AttackShopItem[]>(`${this.apiUrl}/available`);
  }

  purchaseAttack(attackId: number): Observable<PurchaseResponse> {
    return this.http.post<PurchaseResponse>(`${this.apiUrl}/purchase/${attackId}`, {});
  }
}
