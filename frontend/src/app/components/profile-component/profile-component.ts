import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-component.html',
  styleUrls: ['./profile-component.scss']
})
export class ProfileComponent implements OnInit {
  user: any = null;
  loading = true;
  error: string | null = null;
  achievementsCount = 0;
  cosmeticsCount = 0;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.authService.getProfile().subscribe({
      next: (data) => {
        this.user = data;

        // Räkna achievements om JSON finns
        try {
          const ach = JSON.parse(this.user.achievementsJson || '{}');
          this.achievementsCount = Object.keys(ach).length;
        } catch {
          this.achievementsCount = 0;
        }

        // Räkna cosmetics om JSON finns
        try {
          const cos = JSON.parse(this.user.cosmeticItemsJson || '{}');
          this.cosmeticsCount = Object.keys(cos).length;
        } catch {
          this.cosmeticsCount = 0;
        }

        this.loading = false;
      },
      error: (err) => {
        this.error = 'Kunde inte hämta användarinfo';
        this.loading = false;
        console.error(err);
      }
    });
  }
}
