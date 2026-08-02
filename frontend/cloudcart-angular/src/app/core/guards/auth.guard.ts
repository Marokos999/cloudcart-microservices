import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService, keycloak } from '../services/auth';

export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);

  if (keycloak.authenticated) return true;

  const redirectUri = window.location.origin + state.url;
  auth.login(redirectUri);
  return false;
};
