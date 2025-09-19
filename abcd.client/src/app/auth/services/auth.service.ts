import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  token?: string;
  refreshToken?: string;
  message?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient) {
    // Check if user is already authenticated on service initialization
    this.checkAuthenticationStatus();
  }

  signIn(loginData: LoginRequest): Observable<LoginResponse> {
    // Temporary mock for demo purposes - remove when backend is ready
    if (loginData.email === 'demo@test.com' && loginData.password === 'demo123') {
      const mockResponse: LoginResponse = {
        success: true,
        token: 'mock-jwt-token-12345'
      };
      if (mockResponse.token) {
        localStorage.setItem('auth_token', mockResponse.token);
        this.isAuthenticatedSubject.next(true);
      }
      return new Observable(observer => {
        setTimeout(() => {
          observer.next(mockResponse);
          observer.complete();
        }, 1000); // Simulate network delay
      });
    }

    // Real API call for production use
    return this.http.post<LoginResponse>('/api/auth/sign-in', loginData).pipe(
      tap(response => {
        if (response.success && response.token && response.refreshToken) {
          localStorage.setItem('auth_token', response.token);
          this.isAuthenticatedSubject.next(true);
        }
      })
    );
  }

  signOut(): void {
    localStorage.removeItem('auth_token');
    this.isAuthenticatedSubject.next(false);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('auth_token');
  }

  private checkAuthenticationStatus(): void {
    const token = localStorage.getItem('auth_token');
    this.isAuthenticatedSubject.next(!!token);
  }
}
