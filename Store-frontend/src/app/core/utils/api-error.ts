import { HttpErrorResponse } from '@angular/common/http';

/** Field name -> messages, as returned by the validation middleware. */
export type FieldErrors = Readonly<Record<string, readonly string[]>>;

function onlyStrings(values: readonly unknown[]): readonly string[] {
  return values.filter((item): item is string => typeof item === 'string');
}

function readMessage(body: unknown): string | null {
  if (typeof body === 'string' && body.trim()) {
    return body;
  }

  // The validation middleware answers with a bare array of messages.
  if (Array.isArray(body)) {
    const messages = onlyStrings(body);
    return messages.length ? messages.join(' ') : null;
  }

  if (typeof body !== 'object' || body === null) {
    return null;
  }

  const record = body as Record<string, unknown>;
  for (const key of ['message', 'detail', 'title', 'error']) {
    const value = record[key];
    if (typeof value === 'string' && value.trim()) {
      return value;
    }
  }
  return null;
}

/**
 * Pulls per-field messages out of the shapes the API can return:
 * `{ errors: { Name: [...] } }`, `{ Name: [...] }`, or `{ Name: "..." }`.
 * A bare array carries no field names, so it is left to `describeError`.
 */
export function fieldErrors(error: unknown): FieldErrors {
  if (
    !(error instanceof HttpErrorResponse) ||
    typeof error.error !== 'object' ||
    error.error === null ||
    Array.isArray(error.error)
  ) {
    return {};
  }

  const body = error.error as Record<string, unknown>;
  const nested = body['errors'];
  const source =
    typeof nested === 'object' && nested !== null && !Array.isArray(nested)
      ? (nested as Record<string, unknown>)
      : body;

  const collected: Record<string, readonly string[]> = {};
  for (const [field, value] of Object.entries(source)) {
    if (Array.isArray(value)) {
      const messages = onlyStrings(value);
      if (messages.length) {
        collected[field] = messages;
      }
    } else if (typeof value === 'string' && field !== 'message' && field !== 'title') {
      collected[field] = [value];
    }
  }
  return collected;
}

/** A sentence the UI can show for any failed request. */
export function describeError(error: unknown): string | null {
  if (error === undefined || error === null) {
    return null;
  }

  if (!(error instanceof HttpErrorResponse)) {
    return error instanceof Error ? error.message : 'Something went wrong.';
  }

  if (error.status === 0) {
    return 'Cannot reach the Store API. Check that it is running on http://localhost:5127.';
  }

  // Whatever the API said about this specific request beats a generic sentence.
  const messages = Object.values(fieldErrors(error)).flat();
  if (messages.length) {
    return messages.join(' ');
  }

  const reported = readMessage(error.error);
  if (reported) {
    return reported;
  }

  switch (error.status) {
    case 401:
      return 'Your session has expired. Please sign in again.';
    case 403:
      return 'Your account does not have permission to do that.';
    case 404:
      return 'That product no longer exists.';
    case 429:
      return 'Too many attempts. Please wait a moment and try again.';
    default:
      return `Request failed with status ${error.status}.`;
  }
}
