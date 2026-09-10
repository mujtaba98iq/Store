import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthSession } from './auth-store';

export const AUTH_BASE_URL = '/api/auth';

export interface Credentials {
  readonly username: string;
  readonly password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthApi {
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
