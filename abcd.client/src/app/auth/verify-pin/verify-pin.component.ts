import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../../shared/services/notification.service';

@Component({
  selector: 'app-verify-pin',
  standalone: false,
  templateUrl: './verify-pin.component.html',
  styleUrl: './verify-pin.component.css'
})
export class VerifyPinComponent implements OnInit {
  verifyForm!: FormGroup;
  isLoading = false;
  email: string = '';

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    const navigation = this.router.getCurrentNavigation();
    this.email = navigation?.extras?.state?.['email'] ?? '';
  }

  ngOnInit(): void {
    if (!this.email) {
      this.router.navigate(['/auth/login']);
      return;
    }
    this.initializeForm();
  }

  private initializeForm(): void {
    this.verifyForm = this.formBuilder.group({
      pin: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]]
    });
  }

  get pin() {
    return this.verifyForm.get('pin');
  }

  isSubmitDisabled(): boolean {
    return !this.verifyForm.valid || this.isLoading;
  }

  onSubmit(): void {
    if (this.verifyForm.valid) {
      this.isLoading = true;
      const { pin } = this.verifyForm.value;

      this.authService.verifyPin({ email: this.email, pin }).subscribe({
        next: (response) => {
          this.isLoading = false;
          if (response.success) {
            this.notificationService.showSuccess('Login successful!');
            this.router.navigate(['/editor']);
          } else {
            this.handleError('Verification failed. Please try again.');
          }
        },
        error: () => {
          this.isLoading = false;
          this.handleError('Invalid or expired verification code. Please try again.');
        }
      });
    }
  }

  private handleError(message: string): void {
    this.notificationService.showError(message);
    this.verifyForm.reset();
  }
}
