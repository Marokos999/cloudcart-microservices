import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { signal } from '@angular/core';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth';

const makeAuthMock = (authenticated: boolean) => {
  const loginSpy = vi.fn();
  const s = signal(authenticated);
  return {
    service: {
      isAuthenticated: s.asReadonly(),
      login: loginSpy,
    } as unknown as AuthService,
    loginSpy,
  };
};

const runGuard = () =>
  TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

describe('authGuard', () => {
  it('should return true when authenticated', () => {
    const { service } = makeAuthMock(true);
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: service },
        { provide: Router, useValue: { navigate: vi.fn() } },
      ],
    });

    expect(runGuard()).toBe(true);
  });

  it('should return false when not authenticated', () => {
    const { service } = makeAuthMock(false);
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: service },
        { provide: Router, useValue: { navigate: vi.fn() } },
      ],
    });

    expect(runGuard()).toBe(false);
  });

  it('should call login when not authenticated', () => {
    const { service, loginSpy } = makeAuthMock(false);
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: service },
        { provide: Router, useValue: { navigate: vi.fn() } },
      ],
    });

    runGuard();
    expect(loginSpy).toHaveBeenCalledOnce();
  });

  it('should not call login when authenticated', () => {
    const { service, loginSpy } = makeAuthMock(true);
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: service },
        { provide: Router, useValue: { navigate: vi.fn() } },
      ],
    });

    runGuard();
    expect(loginSpy).not.toHaveBeenCalled();
  });
});
