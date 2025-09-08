import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TopCharacterEntry {
  characterName: string;
  userName: string;
  class: string;
  level: number;
  statValue: number;
  statName: string;
  profileIconUrl: string;
  title: string;
}

export interface TopCharactersData {
  kingOfAutobattler: TopCharacterEntry[];
  attackMasters: TopCharacterEntry[];
  tankyBankies: TopCharacterEntry[];
  defenseChampions: TopCharacterEntry[];
  speedDemons: TopCharacterEntry[];
  magicWielders: TopCharacterEntry[];
}

@Injectable({
  providedIn: 'root'
})
export class TopCharactersService {
  private apiUrl = `${environment.apiUrl}/top-characters`;

  constructor(private http: HttpClient) { }

  getTopCharacters(): Observable<TopCharactersData> {
    return this.http.get<TopCharactersData>(this.apiUrl);
  }
}
