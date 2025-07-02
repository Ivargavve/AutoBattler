import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';
import { User } from '../../services/user'; // Justera sökvägen om din User.ts ligger annorlunda

declare const google: any;

@Component({
  selector: 'app-login-form',
  standalone: true,
  templateUrl: './login-form.html',
  styleUrls: ['./login-form.scss'],
})
export class LoginForm implements OnInit {
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback: this.handleGoogleLogin.bind(this),
    });

    google.accounts.id.renderButton(
      document.getElementById('google-button'),
      { theme: 'outline', size: 'large' }
    );
  }

  handleGoogleLogin(response: any): void {
    const idToken = response.credential;

    this.authService.googleLogin(idToken).subscribe({
      next: (res: { token: string; needsUsernameSetup?: boolean; [key: string]: any }) => {
        const { token, needsUsernameSetup, ...rest } = res;
        const userData: User = rest as User;

        localStorage.setItem('token', token);
        this.authService.setUser(userData); // Spara preliminärt user-objekt

        // Hämta sedan färsk profildata och uppdatera BehaviorSubject
        this.authService.getProfile().subscribe({
          next: (profile) => {
            this.authService.setUser(profile); // Uppdatera med backend-data
            if (needsUsernameSetup) {
              this.router.navigate(['/username-form']);
            } else {
              this.router.navigate(['/home']);
            }
          },
          error: () => {
            this.errorMessage = 'Failed to fetch user profile.';
            setTimeout(() => (this.errorMessage = ''), 3000);
          }
        });
      },
      error: () => {
        this.errorMessage = 'Google login failed.';
        setTimeout(() => (this.errorMessage = ''), 3000);
      },
    });
  }


}
