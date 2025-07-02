import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';
import { User } from '../../services/user'; 
import { firstValueFrom } from 'rxjs';

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

  async handleGoogleLogin(response: any): Promise<void> {
    const idToken = response.credential;

    try {
      const res: any = await firstValueFrom(this.authService.googleLogin(idToken));
      const { token, needsUsernameSetup, ...rest } = res;
      const userData: User = rest as User;

      localStorage.setItem('token', token);
      this.authService.setUser(userData);

      const profile = await firstValueFrom(this.authService.getProfile());
      this.authService.setUser(profile);

      if (needsUsernameSetup) {
        await this.router.navigate(['/username-form']);
      } else {
        await this.router.navigate(['/home']);
      }
    } catch (error) {
      this.errorMessage = 'Login failed. Please try again.';
      setTimeout(() => (this.errorMessage = ''), 3000);
    }
  }
}
