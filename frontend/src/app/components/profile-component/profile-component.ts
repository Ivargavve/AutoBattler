import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { User } from '../../services/user';
import { Observable, map } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-component.html',
  styleUrls: ['./profile-component.scss']
})
export class ProfileComponent {
  user$: Observable<User | null>;
  achievementsCount$: Observable<number>;
  cosmeticsCount$: Observable<number>;

  constructor(private authService: AuthService) {
    this.user$ = this.authService.user$;

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
