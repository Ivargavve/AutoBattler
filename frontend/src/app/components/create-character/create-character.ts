import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-create-character',
  standalone: true,
  templateUrl: './create-character.html',
  styleUrls: ['./create-character.scss'],
  imports: [ReactiveFormsModule, CommonModule],
})
export class CreateCharacterComponent {
  characterForm: FormGroup;
  errorMessage = '';
  isSubmitting = false;

  characterClasses = ['Warrior', 'Mage', 'Rogue', 'Cleric'];

  profileIcons = [
    'https://i.imgur.com/2XqffFt.png', // Warrior icon
    'https://i.imgur.com/Rs7bxZT.png', // Mage icon
    'https://i.imgur.com/fP7KCfQ.png', // Rogue icon
    'https://i.imgur.com/fXKMczN.png', // Cleric icon
  ];

  selectedIcon = this.profileIcons[0];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private authService: AuthService
  ) {
    this.characterForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]],
      class: [this.characterClasses[0], Validators.required],
      profileIconUrl: [this.selectedIcon, Validators.required],
    });
  }

  onIconSelect(icon: string) {
    this.selectedIcon = icon;
    this.characterForm.patchValue({ profileIconUrl: icon });
  }

  async onSubmit(): Promise<void> {
    if (this.characterForm.invalid || this.isSubmitting) {
      this.errorMessage = 'Please fill in all fields correctly.';
      setTimeout(() => (this.errorMessage = ''), 3000);
      return;
    }

    this.isSubmitting = true;

    const options = this.authService.getAuthHeaders();

    try {
      // Skapa karaktär på servern
      await firstValueFrom(this.http.post(`${environment.apiUrl}/characters`, this.characterForm.value, options));

      // Uppdatera användarobjektet med ny karaktär
      await this.authService.loadUserWithCharacter();

      // Navigera efter att datan uppdaterats
      await this.router.navigate(['/home']);
    } catch (err: any) {
      this.errorMessage = err.error || 'Failed to create character.';
      setTimeout(() => (this.errorMessage = ''), 3000);
    } finally {
      this.isSubmitting = false;
    }
  }

}
