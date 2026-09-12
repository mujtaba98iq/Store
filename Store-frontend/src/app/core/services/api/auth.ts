import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthSession, Credentials } from '@app/core/models/interfaces/auth-session';
import { ErrorNotification, errorNotification } from '@app/core/utils/error-notification';
import { environment } from '@env/environment';

export const AUTH_BASE_URL = `${environment.apiBaseUrl}/auth`;

/**
 * None of the three is worth a toast. A refused sign-in belongs under the form the
 * reader is looking at, and the refresh and the sign-out call are machinery the
 * reader never asked for - the request that prompted the refresh reports for both.
 */
const silent = () => ({ context: errorNotification(ErrorNotification.Silent) });

/** One method per endpoint of the API's auth resource. */
@Injectable({ providedIn: 'root' })
export class ApiAuthService {
  private readonly http = inject(HttpClient);

  login(credentials: Credentials): Observable<AuthSession> {
    return this.http.post<AuthSession>(`${AUTH_BASE_URL}/login`, credentials, silent());
  }

  refresh(email: string, refreshToken: string): Observable<AuthSession> {
    return this.http.post<AuthSession>(
      `${AUTH_BASE_URL}/refresh`,
      { email, refreshToken },
      silent(),
    );
  }

  logout(email: string, refreshToken: string): Observable<unknown> {
    return this.http.post(`${AUTH_BASE_URL}/logout`, { email, refreshToken }, silent());
  }
}
