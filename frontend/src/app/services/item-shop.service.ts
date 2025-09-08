import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ItemShopItem {
  id: number;
  name: string;
  description: string;
  type: string;
  slot: string;
  rarity: string;
  imageUrl: string;
  statBonuses: { [key: string]: number };
  requiredLevel: number;
  requiredClass: string;
  price: number;
  canAfford: boolean;
  meetsRequirements: boolean;
  isAvailable: boolean;
}

export interface PurchaseItemResponse {
  message: string;
  remainingCredits: number;
  item: any;
}

@Injectable({
  providedIn: 'root'
})
export class ItemShopService {
  private apiUrl = `${environment.apiUrl}/item-shop`;

  constructor(private http: HttpClient) { }

  getAvailableItems(): Observable<ItemShopItem[]> {
    return this.http.get<ItemShopItem[]>(`${this.apiUrl}/available`);
  }

  purchaseItem(itemId: number): Observable<PurchaseItemResponse> {
    return this.http.post<PurchaseItemResponse>(`${this.apiUrl}/purchase/${itemId}`, {});
  }
}
