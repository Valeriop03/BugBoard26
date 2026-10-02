import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const apiUrl = new URL(API_BASE_URL, window.location.origin);
  const requestUrl = new URL(request.url, window.location.origin);
  const apiPath = apiUrl.pathname.replace(/\/$/, '');
  const isApiRequest = requestUrl.origin === apiUrl.origin &&
    (requestUrl.pathname === apiPath || requestUrl.pathname.startsWith(`${apiPath}/`));

  if (!isApiRequest || requestUrl.pathname === `${apiPath}/auth/login`) {
    return next(request);
  }

  const token = authService.getToken();

  if (!token) {
    void router.navigate(['/login']);
  }

  const authenticatedRequest = token ? request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  }) : request;

  return next(authenticatedRequest).pipe(
    catchError(error => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        // Una risposta di una vecchia sessione non deve chiudere un nuovo login.
        if (authService.getToken() === token) {
          authService.logout();
          void router.navigate(['/login']);
        }
      }

      return throwError(() => error);
    })
  );
};
