import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: false
})
export class LoginComponent {
  loginForm: FormGroup;
  errorMessage: string | null = null;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.value;

      // Use AuthService to call the sign-in API
      this.authService.signIn(email, password).subscribe({
        next: (response: any) => {
          console.log('Login successful:', response);
          // Save the token and refresh token to localStorage or a service
          localStorage.setItem('token', response.token);
          localStorage.setItem('refreshToken', response.refreshToken);
          // Navigate to the dashboard in the EditorModule
          this.router.navigate(['/editor/dashboard']);
        },
        error: (error) => {
          console.error('Login failed:', error);
          this.errorMessage = 'Invalid email or password.';
        }
      });
    } else {
      this.errorMessage = 'Please fill in all required fields correctly.';
    }
  }
}
