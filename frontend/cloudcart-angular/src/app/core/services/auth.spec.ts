import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth';

const mockInstance = vi.hoisted(() => ({
  init: vi.fn().mockResolvedValue(false),
  loadUserProfile: vi.fn().mockResolvedValue({ firstName: 'John', lastName: 'Doe', email: 'john@example.com' }),
  login: vi.fn(),
  logout: vi.fn(),
  updateToken: vi.fn().mockResolvedValue(true),
  token: 'mock.token.value',
}));

vi.mock('keycloak-js', () => ({
  default: class {
    get token() { return mockInstance.token; }
    init(...args: any[]) { return mockInstance.init(...args); }
    login(...args: any[]) { return mockInstance.login(...args); }
    logout(...args: any[]) { return mockInstance.logout(...args); }
    updateToken(...args: any[]) { return mockInstance.updateToken(...args); }
    loadUserProfile(...args: any[]) { return mockInstance.loadUserProfile(...args); }
  },
}));

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    vi.clearAllMocks();
    mockInstance.init.mockResolvedValue(false);
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthService);
  });

  it('should start unauthenticated', () => {
    expect(service.isAuthenticated()).toBe(false);
  });

  it('fullName should return empty string when not authenticated', () => {
    expect(service.fullName()).toBe('');
  });

  it('should remain unauthenticated when keycloak init returns false', async () => {
    await service.init();
    expect(service.isAuthenticated()).toBe(false);
  });

  it('should set authenticated to true when keycloak init returns true', async () => {
    mockInstance.init.mockResolvedValue(true);
    await service.init();
    expect(service.isAuthenticated()).toBe(true);
  });

  it('should load profile after successful init', async () => {
    mockInstance.init.mockResolvedValue(true);
    await service.init();
    expect(service.fullName()).toBe('John Doe');
  });

  it('should return token', () => {
    expect(service.getToken()).toBe('mock.token.value');
  });

  it('should handle init failure gracefully', async () => {
    mockInstance.init.mockRejectedValue(new Error('Keycloak unreachable'));
    await service.init();
    expect(service.isAuthenticated()).toBe(false);
  });
});
