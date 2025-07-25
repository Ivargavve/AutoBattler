import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../services/user';
import { Character } from '../../services/character';
import { Observable, of, map, switchMap } from 'rxjs';
import { LoadingSpinnerComponent } from '../loading-component/loading-component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, LoadingSpinnerComponent],
  templateUrl: './profile-component.html',
  styleUrls: ['./profile-component.scss']
})
export class ProfileComponent implements OnInit {
  user$: Observable<User | null> = of(null);
  character$: Observable<Character | null> = of(null);
  achievementsCount$: Observable<number> = of(0);
  cosmeticsCount$: Observable<number> = of(0);

  constructor(
    private authService: AuthService,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.user$ = this.route.paramMap.pipe(
      switchMap(params => {
        const username = params.get('username');
        if (username) {
          return this.authService.getProfileByUsername(username);
        } else {
          return this.authService.user$;
        }
      })
    );

    this.character$ = this.route.paramMap.pipe(
      switchMap(params => {
        const username = params.get('username');
        if (username) {
          return this.authService.getCharacterByUsername(username).pipe(
            map(char => char));
        } else {
          return this.authService.character$;
        }
      })
    );

    this.achievementsCount$ = this.user$.pipe(
      map(user => {
        if (!user) return 0;
        try {
          const ach = JSON.parse(user.achievementsJson || '{}');
          return Object.keys(ach).length;
        } catch {
          return 0;
        }
      })
    );

    this.cosmeticsCount$ = this.user$.pipe(
      map(user => {
        if (!user) return 0;
        try {
          const cos = JSON.parse(user.cosmeticItemsJson || '{}');
          return Object.keys(cos).length;
        } catch {
          return 0;
        }
      })
    );
  }
}
