import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';
import { LoadingSpinnerComponent } from '../loading-component/loading-component';

declare const google: any;

@Component({
  selector: 'app-login-form',
  standalone: true,
  templateUrl: './login-form.html',
  styleUrls: ['./login-form.scss'],
  imports: [CommonModule, LoadingSpinnerComponent],
})
export class LoginForm implements OnInit {
  errorMessage = '';
  loading = false;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback: this.handleGoogleLogin.bind(this),
    });

    google.accounts.id.renderButton(
      document.getElementById('google-button'),
      { theme: 'outline', size: 'large', text: 'signin_with' }
    );
  }

  async handleGoogleLogin(response: any): Promise<void> {
    const idToken = response.credential;
    this.loading = true;

    try {
      const res: any = await firstValueFrom(this.authService.googleLogin(idToken));
      const { token, needsUsernameSetup } = res;
      localStorage.setItem('token', token);
      await this.authService.rechargeCharacter();
      await this.authService.loadUserWithCharacter();

      if (needsUsernameSetup) {
        await this.router.navigate(['/username-form']);
      } else {
        await this.router.navigate(['/home']);
      }
    } catch (error) {
      this.errorMessage = 'Login failed. Please try again.';
      setTimeout(() => (this.errorMessage = ''), 3000);
    } finally {
      this.loading = false;
    }
  }
}
