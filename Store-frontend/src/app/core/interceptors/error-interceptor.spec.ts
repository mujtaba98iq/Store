import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ToastKind } from '@app/core/models/interfaces/toast';
import { AuthStore } from '@app/core/services/common/auth-store';
import { ToastService } from '@app/core/services/common/toast';
import { ErrorNotification, errorNotification } from '@app/core/utils/error-notification';
import { authInterceptor } from './auth-interceptor';
import { errorInterceptor } from './error-interceptor';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let toasts: ToastService;

  const shown = () => toasts.toasts().map((toast) => [toast.kind, toast.message]);

  /** Fires a request that is expected to fail, and answers it with `respond`. */
  const failing = (respond: (request: ReturnType<HttpTestingController['expectOne']>) => void) => {
    let rethrown: unknown;
    http.get('/api/categories/c1').subscribe({ error: (error: unknown) => (rethrown = error) });
    respond(backend.expectOne('/api/categories/c1'));
    return () => rethrown;
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    toasts = TestBed.inject(ToastService);
  });

  afterEach(() => {
    backend.verify();
    toasts.clear();
  });

  it('says nothing about a request that worked', () => {
    http.get('/api/categories').subscribe();
    backend.expectOne('/api/categories').flush({ totalCount: 0, data: [] });

    expect(toasts.toasts()).toEqual([]);
  });

  it('shows the message the API sent rather than a canned one', () => {
    failing((request) =>
      request.flush(
        { message: 'That category still has products filed under it.' },
        { status: 409, statusText: 'Conflict' },
      ),
    );

    expect(shown()).toEqual([
      [ToastKind.Error, 'That category still has products filed under it.'],
    ]);
  });

  it('shows the validation messages a 400 came back with', () => {
    failing((request) =>
      request.flush(
        { errors: { Name: ['Name must be at least 3 characters.'] } },
        { status: 400, statusText: 'Bad Request' },
      ),
    );

    expect(shown()).toEqual([[ToastKind.Error, 'Name must be at least 3 characters.']]);
  });

  it('shows the validation messages a 422 came back with', () => {
    failing((request) =>
      request.flush(['Price must be greater than zero.'], {
        status: 422,
        statusText: 'Unprocessable Content',
      }),
    );

    expect(shown()).toEqual([[ToastKind.Error, 'Price must be greater than zero.']]);
  });

  it('falls back to a sentence of its own for a 404 with no body', () => {
    failing((request) => request.flush(null, { status: 404, statusText: 'Not Found' }));

    expect(shown()).toEqual([[ToastKind.Error, 'That item no longer exists.']]);
  });

  it('reads a forbidden response as an error the reader cannot fix', () => {
    failing((request) => request.flush(null, { status: 403, statusText: 'Forbidden' }));

    expect(shown()).toEqual([
      [ToastKind.Error, 'Your account does not have permission to do that.'],
    ]);
  });

  it('reads an expired session as a warning, not a failure', () => {
    failing((request) => request.flush(null, { status: 401, statusText: 'Unauthorized' }));

    expect(shown()).toEqual([
      [ToastKind.Warning, 'Your session has expired. Please sign in again.'],
    ]);
  });

  it('reads a rate limit as a warning', () => {
    failing((request) => request.flush(null, { status: 429, statusText: 'Too Many Requests' }));

    expect(kindsOnly()).toEqual([ToastKind.Warning]);
  });

  it('reports a dead connection in plain language', () => {
    failing((request) => request.error(new ProgressEvent('error')));

    expect(shown()[0][0]).toBe(ToastKind.Error);
    expect(String(shown()[0][1])).toContain('Cannot reach the Store API');
  });

  it('keeps a server error to one sentence and shows nothing technical', () => {
    failing((request) =>
      request.flush(
        {
          title: 'An unexpected error occurred.',
          stackTrace: 'at Domain.Categories.CategoryService.Delete(Guid id) in /src/...',
          exception: 'System.NullReferenceException',
          connectionString: 'Server=db;Password=hunter2',
        },
        { status: 500, statusText: 'Server Error' },
      ),
    );

    const [kind, message] = shown()[0];
    expect(kind).toBe(ToastKind.Error);
    expect(message).toBe('An unexpected error occurred.');
    expect(message).not.toContain('System.');
    expect(message).not.toContain('hunter2');
  });

  it('leaves the error on its way to the caller', () => {
    const rethrown = failing((request) =>
      request.flush(null, { status: 500, statusText: 'Server Error' }),
    );

    expect(rethrown()).toBeTruthy();
  });

  it('says nothing for a request that asked to be silent', () => {
    http
      .post('/api/auth/login', {}, { context: errorNotification(ErrorNotification.Silent) })
      .subscribe({ error: () => undefined });

    backend.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(toasts.toasts()).toEqual([]);
  });

  it('leaves field messages to the form that can show them beside the field', () => {
    http
      .post('/api/categories', {}, { context: errorNotification(ErrorNotification.FieldsInline) })
      .subscribe({ error: () => undefined });

    backend
      .expectOne('/api/categories')
      .flush({ errors: { Name: ['Already taken.'] } }, { status: 400, statusText: 'Bad Request' });

    expect(toasts.toasts()).toEqual([]);
  });

  it('still speaks up for a form failure that no field can explain', () => {
    http
      .post('/api/categories', {}, { context: errorNotification(ErrorNotification.FieldsInline) })
      .subscribe({ error: () => undefined });

    backend.expectOne('/api/categories').flush(null, { status: 500, statusText: 'Server Error' });

    expect(kindsOnly()).toEqual([ToastKind.Error]);
  });

  it('reports a burst of the same failure once', () => {
    for (const id of ['c1', 'c2', 'c3']) {
      http.delete(`/api/categories/${id}`).subscribe({ error: () => undefined });
      backend
        .expectOne(`/api/categories/${id}`)
        .flush(null, { status: 403, statusText: 'Forbidden' });
    }

    expect(toasts.toasts().length).toBe(1);
    expect(toasts.toasts()[0].repeats).toBe(3);
  });

  function kindsOnly() {
    return toasts.toasts().map((toast) => toast.kind);
  }
});

const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';

/** An unsigned token - the client only ever reads the claims. */
function tokenFor(email: string): string {
  const payload = btoa(JSON.stringify({ [EMAIL_CLAIM]: email, role: 'Admin', exp: 4102444800 }))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');
  return `header.${payload}.signature`;
}

/**
 * The pair, wired the way `appConfig` wires them. The reporter has to sit outside
 * the retrier, or every recovered 401 would be announced as a failure.
 */
describe('errorInterceptor alongside authInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let toasts: ToastService;
  let store: AuthStore;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor, authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    toasts = TestBed.inject(ToastService);
    store = TestBed.inject(AuthStore);
    store.set({ accessToken: tokenFor('a@b.c'), refreshToken: 'r1' });
  });

  afterEach(() => {
    backend.verify();
    toasts.clear();
    localStorage.clear();
  });

  it('says nothing about a 401 the token refresh recovered from', () => {
    http.get('/api/products').subscribe();

    backend.expectOne('/api/products').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend
      .expectOne('/api/auth/refresh')
      .flush({ accessToken: tokenFor('a@b.c'), refreshToken: 'r2' });
    backend.expectOne('/api/products').flush({ totalCount: 0, data: [] });

    expect(toasts.toasts()).toEqual([]);
  });

  it('says nothing when a dead session still leaves the catalogue readable', () => {
    http.get('/api/products').subscribe();

    backend.expectOne('/api/products').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend.expectOne('/api/products').flush({ totalCount: 0, data: [] });

    expect(toasts.toasts()).toEqual([]);
  });

  it('reports a write the dead session cost, once', () => {
    http.post('/api/products', {}).subscribe({ error: () => undefined });

    backend.expectOne('/api/products').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(toasts.toasts().map((toast) => [toast.kind, toast.message])).toEqual([
      [ToastKind.Warning, 'Your session has expired. Please sign in again.'],
    ]);
  });
});
