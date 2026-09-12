import { HttpErrorResponse } from '@angular/common/http';
import { describeError, fieldErrors } from './api-error';

function failure(body: unknown, status = 400): HttpErrorResponse {
  return new HttpErrorResponse({ error: body, status, statusText: 'Bad Request' });
}

describe('fieldErrors', () => {
  it('reads the field map the validation middleware nests under errors', () => {
    expect(fieldErrors(failure({ errors: { Name: ['Too short.'], Price: ['Too low.'] } }))).toEqual(
      {
        Name: ['Too short.'],
        Price: ['Too low.'],
      },
    );
  });

  it('reads a single message per field inside that map', () => {
    expect(fieldErrors(failure({ errors: { Name: 'Too short.' } }))).toEqual({
      Name: ['Too short.'],
    });
  });

  it('reads a top-level field whose messages come as an array', () => {
    expect(fieldErrors(failure({ Name: ['Too short.'] }))).toEqual({ Name: ['Too short.'] });
  });

  it('finds no fields in a bare array of messages', () => {
    expect(fieldErrors(failure(['Too short.']))).toEqual({});
  });

  it('never reads a loose string outside the field map as a message', () => {
    const problem = {
      type: 'https://tools.ietf.org/html/rfc9110#section-15.6.1',
      title: 'An unexpected error occurred.',
      traceId: '00-4bf92f-00f067aa0ba9-01',
      stackTrace: 'at Domain.Categories.CategoryService.Delete(Guid id)',
    };

    expect(fieldErrors(failure(problem, 500))).toEqual({});
  });

  it('finds no fields in something that is not a failed request', () => {
    expect(fieldErrors(new Error('boom'))).toEqual({});
    expect(fieldErrors(null)).toEqual({});
  });
});

describe('describeError', () => {
  it('prefers what the API said about this request', () => {
    expect(describeError(failure({ message: 'That name is already taken.' }, 409))).toBe(
      'That name is already taken.',
    );
  });

  it('joins the validation messages into one sentence', () => {
    expect(describeError(failure(['Name is too short.', 'Price must be positive.'], 422))).toBe(
      'Name is too short. Price must be positive.',
    );
  });

  it('shows the title of a problem document and nothing else from it', () => {
    const message = describeError(
      failure(
        {
          type: 'https://tools.ietf.org/html/rfc9110',
          title: 'An unexpected error occurred.',
          traceId: '00-4bf92f-01',
          stackTrace: 'at Domain.Categories.CategoryService.Delete(Guid id)',
        },
        500,
      ),
    );

    expect(message).toBe('An unexpected error occurred.');
  });

  it('falls back to its own sentence per status when the body says nothing', () => {
    expect(describeError(failure(null, 401))).toContain('session has expired');
    expect(describeError(failure(null, 403))).toContain('permission');
    expect(describeError(failure(null, 404))).toContain('no longer exists');
    expect(describeError(failure(null, 429))).toContain('Too many attempts');
    expect(describeError(failure(null, 500))).toBe('Request failed with status 500.');
  });

  it('names the unreachable API rather than showing a status of zero', () => {
    expect(describeError(failure(null, 0))).toContain('Cannot reach the Store API');
  });

  it('has nothing to say about a request that did not fail', () => {
    expect(describeError(null)).toBeNull();
  });
});
