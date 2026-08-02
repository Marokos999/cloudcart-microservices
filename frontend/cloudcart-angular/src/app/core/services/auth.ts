import { Injectable, signal, computed } from '@angular/core';
import Keycloak from 'keycloak-js';

const keycloakUrl = window.location.hostname === 'localhost'
  ? 'http://localhost:8080'
  : 'http://keycloak.local';

export const keycloak = new Keycloak({
  url: keycloakUrl,
  realm: 'cloudcart',
  clientId: 'cloudcart-client',
});

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _authenticated = signal(false);
  private _profile = signal<{ firstName?: string; lastName?: string; email?: string } | null>(null);

  readonly isAuthenticated = this._authenticated.asReadonly();
  readonly profile = this._profile.asReadonly();
  readonly fullName = computed(() => {
    const p = this._profile();
    if (!p) return '';
    return `${p.firstName ?? ''} ${p.lastName ?? ''}`.trim();
  });

  async init(): Promise<void> {
    try {
      const authenticated = await keycloak.init({
        pkceMethod: 'S256',
        checkLoginIframe: false,
      });

      this._authenticated.set(authenticated);

      if (authenticated) {
        const profile = await keycloak.loadUserProfile();
        this._profile.set({
          firstName: profile.firstName,
          lastName: profile.lastName,
          email: profile.email,
        });
      }
    } catch {
      this._authenticated.set(false);
    }
  }

  login(redirectUri?: string): void {
    keycloak.login({ redirectUri: redirectUri ?? window.location.href });
  }

  register(redirectUri?: string): void {
    keycloak.register({ redirectUri: redirectUri ?? window.location.href });
  }

  logout(): void {
    keycloak.logout({ redirectUri: window.location.origin });
  }

  getToken(): string | undefined {
    return keycloak.token;
  }

  getUsername(): string | undefined {
    return keycloak.tokenParsed?.['preferred_username'] as string | undefined;
  }

  getCustomerId(): string {
    const token = keycloak.token;
    if (!token) return '';
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.sub ?? '';
    } catch { return ''; }
  }

  getRoles(): string[] {
    const access = keycloak.tokenParsed?.['realm_access'] as { roles?: string[] } | undefined;
    return (access?.roles ?? []).filter(
      r => !r.startsWith('default-roles') && r !== 'offline_access' && r !== 'uma_authorization'
    );
  }

  isAdmin(): boolean {
    const token = keycloak.token;
    if (!token) return false;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const roles: string[] = payload?.realm_access?.roles ?? [];
      return roles.includes('admin');
    } catch { return false; }
  }

  async refreshToken(): Promise<boolean> {
    try {
      return await keycloak.updateToken(30);
    } catch {
      return false;
    }
  }
}
