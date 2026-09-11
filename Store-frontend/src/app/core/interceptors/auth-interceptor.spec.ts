import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { authInterceptor } from './auth-interceptor';
import { AuthStore } from '@app/core/services/common/auth-store';

const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';

/** An unsigned token - the client only ever reads the claims. */
function tokenFor(email: string, role = 'Admin'): string {
  const payload = btoa(JSON.stringify({ [EMAIL_CLAIM]: email, role, exp: 4102444800 }))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');
  return `header.${payload}.signature`;
}

describe('authInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let store: AuthStore;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    store = TestBed.inject(AuthStore);
  });

  afterEach(() => {
    backend.verify();
    localStorage.clear();
  });

  it('sends the catalogue request unauthenticated when signed out', () => {
    http.get('/api/products').subscribe();

    const request = backend.expectOne('/api/products');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({ totalCount: 0, data: [] });
  });

  it('attaches the access token when signed in', () => {
    store.set({ accessToken: tokenFor('a@b.c'), refreshToken: 'r1' });

    http.get('/api/products').subscribe();

    const request = backend.expectOne('/api/products');
    expect(request.request.headers.get('Authorization')).toBe(`Bearer ${tokenFor('a@b.c')}`);
    request.flush({ totalCount: 0, data: [] });
  });

  it('never attaches a token to the auth endpoints', () => {
    store.set({ accessToken: tokenFor('a@b.c'), refreshToken: 'r1' });

    http.post('/api/auth/login', {}).subscribe();

    const request = backend.expectOne('/api/auth/login');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({ accessToken: 'x', refreshToken: 'y' });
  });

  it('refreshes once on a 401 and replays the request', () => {
    store.set({ accessToken: tokenFor('a@b.c'), refreshToken: 'r1' });
    const refreshed = tokenFor('a@b.c');

    let result: unknown;
    http.get('/api/products').subscribe((value) => (result = value));

    backend.expectOne('/api/products').flush(null, { status: 401, statusText: 'Unauthorized' });

    const refresh = backend.expectOne('/api/auth/refresh');
    expect(refresh.request.body).toEqual({ email: 'a@b.c', refreshToken: 'r1' });
    refresh.flush({ accessToken: refreshed, refreshToken: 'r2' });

    const replay = backend.expectOne('/api/products');
    expect(replay.request.headers.get('Authorization')).toBe(`Bearer ${refreshed}`);
    replay.flush({ totalCount: 1, data: ['ok'] });

    expect(result).toEqual({ totalCount: 1, data: ['ok'] });
    expect(store.refreshToken()).toBe('r2');
  });

  it('falls back to an anonymous read when the refresh fails', () => {
    store.set({ accessToken: tokenFor('a@b.c'), refreshToken: 'stale' });

    let result: unknown;
    http.get('/api/products').subscribe((value) => (result = value));

    backend.expectOne('/api/products').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });

    const anonymous = backend.expectOne('/api/products');
    expect(anonymous.request.headers.has('Authorization')).toBe(false);
    anonymous.flush({ totalCount: 2, data: ['a', 'b'] });

    expect(result).toEqual({ totalCount: 2, data: ['a', 'b'] });
    expect(store.isSignedIn()).toBe(false);
  });

  it('does not replay a write anonymously', () => {
    store.set({ accessToken: tokenFor('a@b.c'), refreshToken: 'stale' });

    let status: number | undefined;
    http.post('/api/products', {}).subscribe({ error: (e) => (status = e.status) });

    backend.expectOne('/api/products').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(status).toBe(401);
  });
});
