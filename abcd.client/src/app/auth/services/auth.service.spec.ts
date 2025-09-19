import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService, LoginRequest, LoginResponse } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should not be authenticated initially', () => {
    expect(service.isAuthenticated()).toBeFalsy();
    service.isAuthenticated$.subscribe(isAuth => {
      expect(isAuth).toBeFalsy();
    });
  });

  it('should store tokens in memory after successful sign-in', () => {
    const mockLoginData: LoginRequest = {
      email: 'test@example.com',
      password: 'password'
    };
    const mockResponse: LoginResponse = {
      success: true,
      token: 'test-jwt-token',
      refreshToken: 'test-refresh-token'
    };

    service.signIn(mockLoginData).subscribe(response => {
      expect(response).toEqual(mockResponse);
      expect(service.isAuthenticated()).toBeTruthy();
      expect(service.getToken()).toBe('test-jwt-token');
    });

    const req = httpMock.expectOne('/api/auth/sign-in');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should clear tokens from memory on sign-out', () => {
    // First sign in
    const mockResponse: LoginResponse = {
      success: true,
      token: 'test-jwt-token',
      refreshToken: 'test-refresh-token'
    };

    service.signIn({ email: 'test@example.com', password: 'password' }).subscribe();
    const signInReq = httpMock.expectOne('/api/auth/sign-in');
    signInReq.flush(mockResponse);

    // Verify signed in
    expect(service.isAuthenticated()).toBeTruthy();

    // Sign out
    service.signOut();
    
    const signOutReq = httpMock.expectOne('/api/auth/sign-out');
    signOutReq.flush({});

    // Verify signed out and tokens cleared
    expect(service.isAuthenticated()).toBeFalsy();
    expect(service.getToken()).toBeNull();
  });

  it('should handle demo credentials in mock mode', () => {
    const demoLoginData: LoginRequest = {
      email: 'demo@test.com',
      password: 'demo123'
    };

    service.signIn(demoLoginData).subscribe(response => {
      expect(response.success).toBeTruthy();
      expect(response.token).toBe('mock-jwt-token-12345');
      expect(service.isAuthenticated()).toBeTruthy();
    });

    // No HTTP call should be made for demo credentials
    httpMock.expectNone('/api/auth/sign-in');
  });
});
