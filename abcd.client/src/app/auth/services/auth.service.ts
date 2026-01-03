import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, timer } from 'rxjs';
import { tap, switchMap, catchError } from 'rxjs/operators';

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

export interface RefreshTokenRequest {
  email: string;
  refreshToken: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient) {
    // Check authentication status on init by calling backend
    this.checkAuthenticationStatus();
  }

  signIn(loginData: LoginRequest): Observable<LoginResponse> {
    // Real API call with credentials enabled (sends/receives cookies)
    return this.http.post<LoginResponse>('/api/auth/sign-in', loginData, {
      withCredentials: true
    }).pipe(
      tap(response => {
        if (response.success) {
          this.isAuthenticatedSubject.next(true);
        }
      })
    );
  }

  signOut(): void {
    // Call backend to invalidate tokens and clear cookies
    this.http.post('/api/auth/sign-out', {}, {
      withCredentials: true
    }).subscribe({
      next: () => {
        this.isAuthenticatedSubject.next(false);
      },
      error: (error) => {
        console.error('Backend sign-out failed:', error);
        // Continue with client-side cleanup even if backend fails
        this.isAuthenticatedSubject.next(false);
      }
    });
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  // Check authentication status by calling backend /me endpoint
  checkAuthenticationStatus(): void {
    this.http.get<{isAuthenticated: boolean, email?: string}>('/api/auth/me', {
      withCredentials: true
    }).subscribe({
      next: (response) => {
        this.isAuthenticatedSubject.next(response.isAuthenticated);
      },
      error: () => {
        // Not authenticated or error occurred
        this.isAuthenticatedSubject.next(false);
      }
    });
  }

  // Refresh token method - the backend will read refresh token from cookie
  refreshToken(email: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/refresh-token', { email }, {
      withCredentials: true
    }).pipe(
      tap(response => {
        if (response.success) {
          this.isAuthenticatedSubject.next(true);
        }
      }),
      catchError((error) => {
        console.error('Token refresh failed:', error);
        this.isAuthenticatedSubject.next(false);
        throw error;
      })
    );
  }
}
