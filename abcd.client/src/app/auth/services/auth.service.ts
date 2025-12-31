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

  // In-memory token storage
  private currentToken: string | null = null;
  private currentRefreshToken: string | null = null;
  private userEmail: string | null = null;
  private tokenExpirationTime: Date | null = null;
  private refreshTokenExpirationTime: Date | null = null;
  private refreshTimer: any = null;

  // Configuration - should match backend JWT settings in appsettings.json
  private readonly REFRESH_TOKEN_EXPIRY_MINUTES = 60; // RefreshTokenExpiryInMinutes from backend
  private readonly TOKEN_EXPIRY_MINUTES = 30; // TokenExpiryInMinutes from backend
  private readonly REFRESH_BUFFER_MINUTES = 2; // Refresh token 2 minutes before expiry

  constructor(private http: HttpClient) {
    // No need to check localStorage anymore - tokens are only in memory
    this.isAuthenticatedSubject.next(false);
  }

  signIn(loginData: LoginRequest): Observable<LoginResponse> {
    // Real API call for production use
    return this.http.post<LoginResponse>('/api/auth/sign-in', loginData).pipe(
      tap(response => {
        if (response.success && response.token && response.refreshToken) {
          console.log("signIn success " + response.token);
          this.storeTokensInMemory(response.token, response.refreshToken, loginData.email);
          this.isAuthenticatedSubject.next(true);
        }
      })
    );
  }

  signOut(): void {
    // Call backend to invalidate tokens
    if (this.currentToken) {
      this.http.post('/api/auth/sign-out', {}).subscribe({
        next: () => {
          // Backend sign-out successful
        },
        error: (error) => {
          console.error('Backend sign-out failed:', error);
          // Continue with client-side cleanup even if backend fails
        }
      });
    }
    
    this.clearTokensFromMemory();
    this.isAuthenticatedSubject.next(false);
  }

  isAuthenticated(): boolean {
    console.log("current token: " + this.currentToken + ", is valid: " + this.isTokenValid());
    return this.currentToken !== null && this.isTokenValid();
  }

  getToken(): string | null {
    if (this.isTokenValid()) {
      console.log("token is valid");
      return this.currentToken;
    }
    console.log("token is not valid");
    return null;
  }

  private storeTokensInMemory(token: string, refreshToken: string, email: string): void {
    this.currentToken = token;
    this.currentRefreshToken = refreshToken;
    this.userEmail = email;
    
    // Set token expiration times
    const now = new Date();
    this.tokenExpirationTime = new Date(now.getTime() + (this.TOKEN_EXPIRY_MINUTES * 60 * 1000));
    this.refreshTokenExpirationTime = new Date(now.getTime() + (this.REFRESH_TOKEN_EXPIRY_MINUTES * 60 * 1000));
    
    // Set up automatic token refresh
    this.scheduleTokenRefresh();
  }

  private clearTokensFromMemory(): void {
    this.currentToken = null;
    this.currentRefreshToken = null;
    this.userEmail = null;
    this.tokenExpirationTime = null;
    this.refreshTokenExpirationTime = null;
    
    // Clear any pending refresh timer
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
      this.refreshTimer = null;
    }
  }

  private isTokenValid(): boolean {
    if (!this.currentToken || !this.tokenExpirationTime) {
      return false;
    }
    return new Date() < this.tokenExpirationTime;
  }

  private isRefreshTokenValid(): boolean {
    if (!this.currentRefreshToken || !this.refreshTokenExpirationTime) {
      return false;
    }
    return new Date() < this.refreshTokenExpirationTime;
  }

  private scheduleTokenRefresh(): void {
    if (!this.tokenExpirationTime) {
      return;
    }

    // Clear any existing timer
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
    }

    // Calculate when to refresh (buffer time before expiration)
    const refreshTime = new Date(this.tokenExpirationTime.getTime() - (this.REFRESH_BUFFER_MINUTES * 60 * 1000));
    const timeUntilRefresh = refreshTime.getTime() - new Date().getTime();

    if (timeUntilRefresh > 0) {
      this.refreshTimer = setTimeout(() => {
        this.refreshAccessToken();
      }, timeUntilRefresh);
    } else {
      // Token is about to expire or already expired, refresh immediately
      this.refreshAccessToken();
    }
  }

  private refreshAccessToken(): void {
    if (!this.isRefreshTokenValid() || !this.currentRefreshToken || !this.userEmail) {
      console.log("refreshAccessToken - signing out");
      // Refresh token is expired or missing, force logout
      this.signOut();
      return;
    }

    const refreshRequest: RefreshTokenRequest = {
      email: this.userEmail,
      refreshToken: this.currentRefreshToken
    };

    this.http.post<LoginResponse>('/api/auth/refresh-token', refreshRequest).pipe(
      catchError((error) => {
        console.error('Token refresh failed:', error);
        // Force logout on refresh failure
        this.signOut();
        throw error;
      })
    ).subscribe(response => {
      if (response.success && response.token && response.refreshToken) {
        // Update tokens in memory
        this.storeTokensInMemory(response.token, response.refreshToken, this.userEmail!);
      } else {
        // Invalid response, force logout
        this.signOut();
      }
    });
  }
}
