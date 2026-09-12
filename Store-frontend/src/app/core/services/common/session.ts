import { Injectable, inject } from '@angular/core';
import { Observable, catchError, finalize, shareReplay, tap, throwError } from 'rxjs';
import { AuthSession, Credentials } from '@app/core/models/interfaces/auth-session';
import { ApiAuthService } from '@app/core/services/api/auth';
import { AuthStore } from './auth-store';
import { ToastService } from './toast';

/** Sign-in, sign-out, and a single-flight access-token refresh. */
@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly api = inject(ApiAuthService);
  private readonly store = inject(AuthStore);
  private readonly toasts = inject(ToastService);

  /** Shared while a refresh is in flight so parallel 401s trigger only one call. */
  private inFlight: Observable<AuthSession> | null = null;

  signIn(credentials: Credentials): Observable<AuthSession> {
    return this.api.login(credentials).pipe(
      tap((session) => {
        this.store.set(session);
        // Announced here rather than on the sign-in page: the page navigates away
        // the moment this resolves, and the toast outlives the navigation.
        this.toasts.success('Signed in successfully.');
      }),
    );
  }

  signOut(): void {
    const email = this.store.email();
    const refreshToken = this.store.refreshToken();
    this.store.clear();
    this.toasts.info('You have been signed out.');

    if (email && refreshToken) {
      // Best effort - the client is signed out either way.
      this.api.logout(email, refreshToken).subscribe({ error: () => undefined });
    }
  }

  refresh(): Observable<AuthSession> {
    if (this.inFlight) {
      return this.inFlight;
    }

    const email = this.store.email();
    const refreshToken = this.store.refreshToken();
    if (!email || !refreshToken) {
      return throwError(() => new Error('No refresh token available.'));
    }

    this.inFlight = this.api.refresh(email, refreshToken).pipe(
      tap((session) => this.store.set(session)),
      catchError((error: unknown) => {
        this.store.clear();
        return throwError(() => error);
      }),
      finalize(() => {
        this.inFlight = null;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );

    return this.inFlight;
  }
}
