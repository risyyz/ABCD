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
    return this.http.post<LoginResponse>('/api/auth/sign-in', loginData).pipe(
      tap(response => {
        if (response.success && response.token) {
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
