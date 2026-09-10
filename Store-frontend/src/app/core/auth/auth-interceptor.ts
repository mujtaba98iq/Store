import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AUTH_BASE_URL } from './auth-api';
import { AuthStore } from './auth-store';
import { Session } from './session';

function withBearer<T>(request: HttpRequest<T>, token: string): HttpRequest<T> {
  return request.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

/**
 * Attaches the access token and, on a 401, refreshes it once and replays the
 * request. If the refresh itself fails, a read is replayed anonymously - the
 * catalogue is public, so a dead session should not hide it. Calls to the auth
 * endpoints are left alone.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  if (request.url.startsWith(AUTH_BASE_URL)) {
    return next(request);
  }

  const store = inject(AuthStore);
  const session = inject(Session);

  const token = store.accessToken();
  if (!token) {
    return next(request);
  }

  return next(withBearer(request, token)).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401) {
        return throwError(() => error);
      }

      const retryAnonymously = () =>
        request.method === 'GET' ? next(request) : throwError(() => error);

      if (!store.refreshToken()) {
        return retryAnonymously();
      }

      return session.refresh().pipe(
        switchMap((refreshed) => next(withBearer(request, refreshed.accessToken))),
        catchError(() => retryAnonymously()),
      );
    }),
  );
};
