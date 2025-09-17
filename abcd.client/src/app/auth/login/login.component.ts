import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../services/auth.service';

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
    private snackBar: MatSnackBar
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
            this.snackBar.open('Login successful!', 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar']
            });
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
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
    this.clearForm();
  }

  private clearForm(): void {
    this.loginForm.reset();
    this.isCaptchaChecked = false;
  }
}
