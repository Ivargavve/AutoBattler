import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Injectable({
  providedIn: 'root'
})
export class TitleService {
  private baseTitle = 'AutoBattler';

  constructor(private title: Title) {}

  setTitle(pageTitle: string) {
    this.title.setTitle(`${pageTitle} | ${this.baseTitle}`);
  }

  setBaseTitle() {
    this.title.setTitle(this.baseTitle);
  }
}
