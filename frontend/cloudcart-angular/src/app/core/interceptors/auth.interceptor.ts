import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth';
import { ToastService } from '../services/toast';

function addToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const toast = inject(ToastService);

  const token = auth.getToken();
  const authReq = token ? addToken(req, token) : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        return from(auth.refreshToken()).pipe(
          switchMap((refreshed) => {
            const newToken = auth.getToken();
            if (refreshed && newToken) {
              return next(addToken(req, newToken));
            }
            toast.show('Session expired. Please sign in.', 'error');
            return throwError(() => err);
          })
        );
      }
      if (err.status === 403) toast.show('Access denied.', 'error');
      else if (err.status === 0) toast.show('Cannot reach server.', 'error');
      else if (err.status >= 500) toast.show('Server error. Try again later.', 'error');
      return throwError(() => err);
    })
  );
};
