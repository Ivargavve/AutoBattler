import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { Character } from '../../services/character';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-hero',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hero-component.html',
  styleUrls: ['./hero-component.scss']
})
export class HeroComponent {
  character$: Observable<Character | null>;

  constructor(private authService: AuthService) {
    this.character$ = this.authService.character$;
  }
}
