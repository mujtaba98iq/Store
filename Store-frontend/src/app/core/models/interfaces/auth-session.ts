/** What `POST /auth/login` and `POST /auth/refresh` answer with. */
export interface AuthSession {
  readonly accessToken: string;
  readonly refreshToken: string;
}

/** The body `POST /auth/login` binds. */
export interface Credentials {
  readonly username: string;
  readonly password: string;
}

/** The claims the client reads out of an access token. */
export interface AccessTokenClaims {
  readonly userId: string;
  readonly email: string;
  readonly roles: readonly string[];
  /** Expiry in epoch milliseconds, or `null` when the token carries no `exp`. */
  readonly expiresAt: number | null;
}
