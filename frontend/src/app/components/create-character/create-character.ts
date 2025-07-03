import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { firstValueFrom, Subscription } from 'rxjs';

@Component({
  selector: 'app-create-character',
  standalone: true,
  templateUrl: './create-character.html',
  styleUrls: ['./create-character.scss'],
  imports: [ReactiveFormsModule, CommonModule],
})
export class CreateCharacterComponent implements OnInit {
  characterForm: FormGroup;
  errorMessage = '';
  isSubmitting = false;

  characterClasses = ['Warrior', 'Mage', 'Rogue', 'Cleric', 'Paladin', 'Druid', 'Hunter', 'Sorcerer', 'Bard', 'Necromancer', 
    'Monk', 'Warlock', 'Ranger', 'Assassin', 'Shaman', 'Berserker', 'Knight', 'Priest', 'Wizard', 'Alchemist'];
  profileIcons = [
    'assets/char1.jpeg', 'assets/char2.jpeg', 'assets/char3.jpeg',
    'assets/char4.jpeg', 'assets/char5.jpeg', 'assets/char6.jpeg', 'assets/char7.jpeg',
    'assets/char8.jpeg', 'assets/char9.jpeg', 'assets/char10.jpeg', 'assets/char11.jpeg',
    'assets/char12.jpeg', 'assets/char13.jpeg', 'assets/char14.jpeg', 'assets/char15.jpeg'
  ];
  selectedIcon = this.profileIcons[0];

  hasCharacter = false;
  private userSub?: Subscription;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private authService: AuthService
  ) {
    this.characterForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(13)]],
      class: [this.characterClasses[0], Validators.required],
      profileIconUrl: [this.selectedIcon, Validators.required],
    });
  }

  ngOnInit(): void {
    this.userSub = this.authService.user$.subscribe(user => {
      this.hasCharacter = !!user?.character;
    });
  }

  ngOnDestroy(): void {
    this.userSub?.unsubscribe();
  }

  onIconSelect(icon: string) {
    this.selectedIcon = icon;
    this.characterForm.patchValue({ profileIconUrl: icon });
  }

  async onSubmit(): Promise<void> {
    if (this.characterForm.invalid || this.isSubmitting || this.hasCharacter) {
      this.errorMessage = this.hasCharacter
        ? 'You already have a character!'
        : 'Please fill in all fields correctly.';
      setTimeout(() => (this.errorMessage = ''), 3000);
      return;
    }
    this.isSubmitting = true;
    const options = this.authService.getAuthHeaders();

    try {
      await firstValueFrom(this.http.post(`${environment.apiUrl}/characters`, this.characterForm.value, options));
      await this.authService.loadUserWithCharacter();
      await this.router.navigate(['/home']);
    } catch (err: any) {
      this.errorMessage = err.error || 'Failed to create character.';
      setTimeout(() => (this.errorMessage = ''), 3000);
    } finally {
      this.isSubmitting = false;
    }
  }
}
