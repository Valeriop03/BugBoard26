import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const currentUser = authService.getCurrentUser();

  if (!authService.isLoggedIn()) {
    return router.createUrlTree(['/login']);
  }

  if (currentUser?.role === 'ADMIN') {
    return true;
  }

  return router.createUrlTree(['/issues']);
};

export const issueCreationGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    return router.createUrlTree(['/login']);
  }

  const role = authService.getCurrentUser()?.role;

  return role === 'ADMIN' || role === 'USER'
    ? true
    : router.createUrlTree(['/issues']);
};
