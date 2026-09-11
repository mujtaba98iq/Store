import { AccessTokenClaims } from '@app/core/models/interfaces/auth-session';

/**
 * Claim names emitted by the API. The bearer setup runs with
 * `MapInboundClaims = false`, so the raw WS-Federation URIs reach the client.
 */
const USER_ID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
const ROLE_CLAIM = 'role';

function decodePayload(token: string): Record<string, unknown> | null {
  const segment = token.split('.')[1];
  if (!segment) {
    return null;
  }

  try {
    const base64 = segment.replace(/-/g, '+').replace(/_/g, '/');
    const binary = atob(base64.padEnd(Math.ceil(base64.length / 4) * 4, '='));
    const bytes = Uint8Array.from(binary, (character) => character.charCodeAt(0));
    const parsed: unknown = JSON.parse(new TextDecoder().decode(bytes));
    return typeof parsed === 'object' && parsed !== null
      ? (parsed as Record<string, unknown>)
      : null;
  } catch {
    return null;
  }
}

function asStrings(value: unknown): readonly string[] {
  if (typeof value === 'string') {
    return [value];
  }
  return Array.isArray(value) ? value.filter((item) => typeof item === 'string') : [];
}

export function readAccessToken(token: string): AccessTokenClaims | null {
  const payload = decodePayload(token);
  if (!payload) {
    return null;
  }

  const expiry = payload['exp'];

  return {
    userId: typeof payload[USER_ID_CLAIM] === 'string' ? payload[USER_ID_CLAIM] : '',
    email: typeof payload[EMAIL_CLAIM] === 'string' ? payload[EMAIL_CLAIM] : '',
    roles: asStrings(payload[ROLE_CLAIM]),
    expiresAt: typeof expiry === 'number' ? expiry * 1000 : null,
  };
}
