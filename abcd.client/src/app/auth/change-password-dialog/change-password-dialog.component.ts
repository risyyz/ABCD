import { Component, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-change-password-dialog',
  standalone: false,
  templateUrl: './change-password-dialog.component.html',
  styleUrl: './change-password-dialog.component.scss'
})
export class ChangePasswordDialogComponent {
  @Output() close = new EventEmitter<void>();

  step: 'request-pin' | 'change-password' = 'request-pin';
  pin = '';
  newPassword = '';
  confirmPassword = '';
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(private http: HttpClient) {}

  requestPin(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.http.post<{ success: boolean }>('/api/auth/password', {}, { withCredentials: true })
      .subscribe({
        next: () => {
          this.isLoading = false;
          this.step = 'change-password';
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = 'Failed to send verification code. Please try again.';
        }
      });
  }

  get passwordsMatch(): boolean {
    return this.newPassword === this.confirmPassword;
  }

  get passwordValid(): boolean {
    return this.newPassword.length >= 8
      && /[A-Z]/.test(this.newPassword)
      && /[a-z]/.test(this.newPassword)
      && /\d/.test(this.newPassword)
      && /[^A-Za-z0-9]/.test(this.newPassword);
  }

  get canSubmit(): boolean {
    return this.pin.length === 6
      && this.passwordValid
      && this.passwordsMatch
      && !this.isLoading;
  }

  changePassword(): void {
    if (!this.canSubmit) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.http.put<{ success: boolean }>('/api/auth/password', {
      pin: this.pin,
      newPassword: this.newPassword
    }, { withCredentials: true })
      .subscribe({
        next: () => {
          this.isLoading = false;
          this.successMessage = 'Password changed successfully!';
          setTimeout(() => this.close.emit(), 1500);
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error || 'Failed to change password. Please try again.';
        }
      });
  }

  onClose(): void {
    this.close.emit();
  }
}
