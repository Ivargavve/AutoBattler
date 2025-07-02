import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-username-setup',
  standalone: true,
  templateUrl: './username-form.html',
  styleUrls: ['./username-form.scss'],
  imports: [ReactiveFormsModule, CommonModule],
})
export class UsernameSetupComponent {
  usernameForm: FormGroup;
  errorMessage = '';
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private http: HttpClient
  ) {
    this.usernameForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]],
    });
  }

  async onSubmit(): Promise<void> {
    if (this.usernameForm.invalid || this.isSubmitting) {
      this.errorMessage = 'Please enter a valid username (3–20 characters).';
      setTimeout(() => (this.errorMessage = ''), 3000);
      return;
    }

    this.isSubmitting = true;

    const userId = this.getUserIdFromToken();
    const newUsername = this.usernameForm.value.username.trim();

    if (!userId) {
      this.errorMessage = 'User ID not found. Please login again.';
      this.isSubmitting = false;
      return;
    }

    try {
      await firstValueFrom(this.http.put(`${environment.apiUrl}/googleauth/set-username`, { userId, newUsername }));
      
      await this.authService.loadUserWithCharacter();
      await firstValueFrom(this.authService.user$);
      await this.router.navigate(['/home']);
    } catch (err: any) {
      this.errorMessage = err.error || 'Failed to update username.';
      setTimeout(() => (this.errorMessage = ''), 3000);
    } finally {
      this.isSubmitting = false;
    }
  }

  private getUserIdFromToken(): number | null {
    const token = this.authService.getToken();
    if (!token) return null;

    try {
      const payloadBase64 = token.split('.')[1];
      const payloadJson = atob(payloadBase64);
      const payload = JSON.parse(payloadJson);

      return Number(
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
        payload['nameid'] ||
        payload['sub'] ||
        null
      );
    } catch {
      return null;
    }
  }
}
