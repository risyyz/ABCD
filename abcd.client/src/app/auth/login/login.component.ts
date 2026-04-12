import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ReCaptchaV3Service } from 'ngx-recaptcha';
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

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService,
    private recaptchaV3Service: ReCaptchaV3Service
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  get email() {
    return this.loginForm.get('email');
  }

  get password() {
    return this.loginForm.get('password');
  }

  isSignInDisabled(): boolean {
    return !this.loginForm.valid || this.isLoading;
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      
      // Execute reCAPTCHA v3
      this.recaptchaV3Service.execute('login').subscribe({
        next: (token: string) => {
          const { email, password } = this.loginForm.value;

          this.authService.signIn({ email, password, recaptchaToken: token }).subscribe({
            next: (response) => {
              this.isLoading = false;
              if (response.requiresTwoFactor) {
                this.router.navigate(['/auth/verify-pin'], { state: { email: response.email } });
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
        },
        error: (error) => {
          this.isLoading = false;
          this.handleLoginError('Failed to verify reCAPTCHA. Please try again.');
          console.error('reCAPTCHA error:', error);
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
  }
}
