import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthSession, Credentials } from '@app/core/models/interfaces/auth-session';
import { environment } from '@env/environment';

export const AUTH_BASE_URL = `${environment.apiBaseUrl}/auth`;

/** One method per endpoint of the API's auth resource. */
@Injectable({ providedIn: 'root' })
export class ApiAuthService {
  private readonly http = inject(HttpClient);

  login(credentials: Credentials): Observable<AuthSession> {
    return this.http.post<AuthSession>(`${AUTH_BASE_URL}/login`, credentials);
  }

  refresh(email: string, refreshToken: string): Observable<AuthSession> {
    return this.http.post<AuthSession>(`${AUTH_BASE_URL}/refresh`, { email, refreshToken });
  }

  logout(email: string, refreshToken: string): Observable<unknown> {
    return this.http.post(`${AUTH_BASE_URL}/logout`, { email, refreshToken });
  }
}
