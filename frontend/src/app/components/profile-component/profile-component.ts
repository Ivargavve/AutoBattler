import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { User } from '../../services/user';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-component.html',
  styleUrls: ['./profile-component.scss']
})
export class ProfileComponent implements OnInit {
  user$: Observable<User | null>;
  achievementsCount = 0;
  cosmeticsCount = 0;

  constructor(private authService: AuthService) {
    this.user$ = this.authService.user$;
  }

  ngOnInit(): void {
    this.user$.subscribe((user) => {
      if (user) {
        try {
          const ach = JSON.parse(user.achievementsJson || '{}');
          this.achievementsCount = Object.keys(ach).length;
        } catch {
          this.achievementsCount = 0;
        }

        try {
          const cos = JSON.parse(user.cosmeticItemsJson || '{}');
          this.cosmeticsCount = Object.keys(cos).length;
        } catch {
          this.cosmeticsCount = 0;
        }
      }
    });
  }
}
