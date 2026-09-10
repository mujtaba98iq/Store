import { Injectable, computed, signal } from '@angular/core';
import { AccessTokenClaims, readAccessToken } from './jwt';

const STORAGE_KEY = 'STOR.session';

export interface AuthSession {
  readonly accessToken: string;
  readonly refreshToken: string;
}

function readStoredSession(): AuthSession | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }

    const parsed: unknown = JSON.parse(raw);
    if (typeof parsed !== 'object' || parsed === null) {
      return null;
    }

    const { accessToken, refreshToken } = parsed as Partial<AuthSession>;
    return typeof accessToken === 'string' && typeof refreshToken === 'string'
      ? { accessToken, refreshToken }
      : null;
  } catch {
    // Private browsing or blocked storage - start signed out.
    return null;
  }
}

/** Holds the signed-in session and mirrors it to `localStorage`. */
@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly session = signal<AuthSession | null>(readStoredSession());

  readonly accessToken = computed(() => this.session()?.accessToken ?? null);
  readonly refreshToken = computed(() => this.session()?.refreshToken ?? null);

  readonly claims = computed<AccessTokenClaims | null>(() => {
    const token = this.accessToken();
    return token ? readAccessToken(token) : null;
  });

  readonly isSignedIn = computed(() => this.session() !== null);
  readonly email = computed(() => this.claims()?.email ?? null);
  readonly isAdmin = computed(() => this.claims()?.roles.includes('Admin') ?? false);

  set(session: AuthSession): void {
    this.session.set(session);
    this.persist(session);
  }

  clear(): void {
    this.session.set(null);
    this.persist(null);
  }

  private persist(session: AuthSession | null): void {
    try {
      if (session) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
      } else {
        localStorage.removeItem(STORAGE_KEY);
      }
    } catch {
      // Storage is unavailable; the session still lives for this tab.
    }
  }
}
