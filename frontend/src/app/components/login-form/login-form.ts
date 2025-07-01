import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RouterModule } from '@angular/router';

declare const google: any;

@Component({
  selector: 'app-login-form',
  standalone: true,
  templateUrl: './login-form.html',
  styleUrls: ['./login-form.scss'],
  imports: [
    ReactiveFormsModule,
    FormsModule,
    CommonModule,
    RouterModule
  ],
})
export class LoginForm implements OnInit {
  loginForm: FormGroup;
  errorMessage: string = '';
  showPassword: boolean = false;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    google.accounts.id.initialize({
      client_id: '334708074423-rmdjlkamju4u8sjvjq9qhui4up47e23f.apps.googleusercontent.com',
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
      next: (res) => {
        localStorage.setItem('token', res.token);
        this.router.navigate(['/']);
      },
      error: () => {
        this.errorMessage = 'Google login failed.';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.loginForm.invalid || this.isSubmitting) {
      this.errorMessage = 'Please fill in all required fields.';
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    this.isSubmitting = true;
    const { username, password } = this.loginForm.value;

    this.authService.login(username, password).subscribe({
      next: (response) => {
        localStorage.setItem('token', response.token);
        this.isSubmitting = false;
        this.router.navigate(['/']);
      },
      error: (err) => {
        const errorText = typeof err.error === 'string' ? err.error.toLowerCase() : '';

        if (errorText.includes('invalid') || errorText.includes('wrong')) {
          this.errorMessage = 'Wrong username or password.';
        } else if (errorText.includes('not found')) {
          this.errorMessage = 'User not found.';
        } else {
          this.errorMessage = 'Login failed.';
        }

        this.isSubmitting = false;
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }
}
