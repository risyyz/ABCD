import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../../shared/services/notification.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  isCaptchaChecked = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      captcha: [false, [Validators.requiredTrue]]
    });
  }

  get email() {
    return this.loginForm.get('email');
  }

  get password() {
    return this.loginForm.get('password');
  }

  get captcha() {
    return this.loginForm.get('captcha');
  }

  onCaptchaChange(checked: boolean): void {
    this.isCaptchaChecked = checked;
    this.loginForm.patchValue({ captcha: checked });
  }

  isSignInDisabled(): boolean {
    return !this.loginForm.valid || this.isLoading;
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      const { email, password } = this.loginForm.value;

      this.authService.signIn({ email, password }).subscribe({
        next: (response) => {
          this.isLoading = false;
          if (response.success) {
            this.notificationService.showSuccess('Login successful!');
            this.router.navigate(['/editor']);
          } else {
            this.handleLoginError(response.message || 'Login failed');
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.handleLoginError('Login failed. Please check your credentials.');
          console.error('Login error:', error);
        }
      });
    }
  }

  private handleLoginError(message: string): void {
    this.notificationService.showError(message);
    this.clearForm();
  }

  private clearForm(): void {
    this.loginForm.reset();
    this.isCaptchaChecked = false;
  }
}
