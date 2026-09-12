import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { errorInterceptor } from '@app/core/interceptors/error-interceptor';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { ToastService } from '@app/core/services/common/toast';
import { User } from '../../models/user.model';
import { Users } from './users';

function user(overrides: Partial<User> = {}): User {
  return {
    id: 'u1',
    username: 'noor',
    role: 'User',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    createdById: 'u0',
    updatedById: null,
    ...overrides,
  };
}

function page<T>(data: readonly T[], totalCount = data.length): PaginatedResult<T> {
  return { totalCount, data };
}

describe('Users', () => {
  let fixture: ComponentFixture<Users>;
  let element: HTMLElement;
  let http: HttpTestingController;
  let toasts: ToastService;

  const rowNames = () =>
    Array.from(element.querySelectorAll('.table__name')).map((node) => node.textContent?.trim());

  const buttonLabelled = (label: string) =>
    Array.from(element.querySelectorAll<HTMLButtonElement>('button')).find(
      (button) => button.textContent?.trim() === label,
    );

  /**
   * Resource requests are issued from effects, so change detection has to run -
   * but never await stability first: an open request keeps the app unstable.
   */
  const pendingRequests = (method: string) => {
    fixture.detectChanges();
    return http.match(
      (request) => request.url.startsWith('/api/users') && request.method === method,
    );
  };

  const render = async () => {
    await Promise.resolve();
    fixture.detectChanges();
  };

  /** Answers the pending listing request and re-renders with the result. */
  const flushList = async (result: PaginatedResult<User>) => {
    const latest = pendingRequests('GET').at(-1);
    latest?.flush(result);
    await render();
    return latest;
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Users],
      providers: [
        // The page reports failures through the interceptor, so it is part of
        // what these tests exercise.
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    toasts = TestBed.inject(ToastService);
    fixture = TestBed.createComponent(Users);
    element = fixture.nativeElement as HTMLElement;
  });

  afterEach(() => {
    http.verify({ ignoreCancelled: true });
    toasts.clear();
  });

  it('lists the page the API returns', async () => {
    await flushList(page([user(), user({ id: 'u2', username: 'sara', role: 'Admin' })]));

    expect(rowNames()).toEqual(['noor', 'sara']);
    expect(element.querySelector('.admin__count')?.textContent).toContain('2 users');
  });

  it('asks the API for the first page ordered newest first', async () => {
    const request = await flushList(page([user()]));

    expect(request?.request.params.get('Page')).toBe('1');
    expect(request?.request.params.get('PageSize')).toBe('10');
    expect(request?.request.params.get('OrderBy')).toBe('1');
    expect(request?.request.params.get('OrderByDirection')).toBe('1');
    expect(request?.request.params.has('Username')).toBe(false);
    expect(request?.request.params.has('Role')).toBe(false);
  });

  it('sends the typed term as the Username filter', async () => {
    await flushList(page([user()]));

    const search = element.querySelector<HTMLInputElement>('#user-search');
    search!.value = 'sar';
    search!.dispatchEvent(new Event('input'));

    // The term is debounced, so let the timer and the effect settle.
    await new Promise((resolve) => setTimeout(resolve, 350));
    const request = await flushList(page([user()]));

    expect(request?.request.params.get('Username')).toBe('sar');
  });

  it('sends the chosen role pill as the Role filter, and drops it again', async () => {
    await flushList(page([user()]));

    buttonLabelled('Admins')!.click();
    const filtered = await flushList(page([user({ role: 'Admin' })]));
    expect(filtered?.request.params.get('Role')).toBe('Admin');

    buttonLabelled('Everyone')!.click();
    const unfiltered = await flushList(page([user()]));
    expect(unfiltered?.request.params.has('Role')).toBe(false);
  });

  it('translates the sort choice into OrderBy and OrderByDirection', async () => {
    await flushList(page([user()]));

    const sort = element.querySelector<HTMLSelectElement>('#user-sort');
    sort!.value = 'username-asc';
    sort!.dispatchEvent(new Event('change'));

    const request = await flushList(page([user()]));
    expect(request?.request.params.get('OrderBy')).toBe('2');
    expect(request?.request.params.get('OrderByDirection')).toBe('0');
  });

  it('pages through the results', async () => {
    await flushList(page([user()], 30));

    element.querySelector<HTMLButtonElement>('[aria-label="Next page"]')!.click();

    const request = await flushList(page([user()], 30));
    expect(request?.request.params.get('Page')).toBe('2');
  });

  it('creates a user from the form', async () => {
    await flushList(page([user()]));

    buttonLabelled('New user')!.click();
    await render();

    const username = element.querySelector<HTMLInputElement>('#user-username');
    username!.value = 'sara';
    username!.dispatchEvent(new Event('input'));

    const password = element.querySelector<HTMLInputElement>('#user-password');
    password!.value = 'sup3rsecret';
    password!.dispatchEvent(new Event('input'));

    const role = element.querySelector<HTMLSelectElement>('#user-role');
    role!.value = 'Admin';
    role!.dispatchEvent(new Event('change'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    const posted = pendingRequests('POST').at(-1);
    expect(posted?.request.body).toEqual({
      username: 'sara',
      password: 'sup3rsecret',
      role: 'Admin',
    });

    posted?.flush(user({ id: 'u2', username: 'sara', role: 'Admin' }));
    await render();

    // The dialog closes and the listing refetches.
    expect(element.querySelector('app-user-form')).toBeNull();
    await flushList(page([user(), user({ id: 'u2', username: 'sara', role: 'Admin' })]));
    expect(rowNames()).toContain('sara');
  });

  it('leaves the password out of an edit that did not set a new one', async () => {
    await flushList(page([user()]));

    buttonLabelled('Edit')!.click();
    await render();

    const username = element.querySelector<HTMLInputElement>('#user-username');
    expect(username?.value).toBe('noor');
    // The API only ever holds the hash, so the box starts empty on an edit.
    expect(element.querySelector<HTMLInputElement>('#user-password')?.value).toBe('');

    username!.value = 'noora';
    username!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    const patched = pendingRequests('PATCH').at(-1);
    expect(patched?.request.url).toBe('/api/users/u1');
    expect(patched?.request.body).toEqual({ username: 'noora', role: 'User' });

    patched?.flush(user({ username: 'noora' }));
    await render();
    await flushList(page([user({ username: 'noora' })]));
  });

  it('sends the password when the edit set one', async () => {
    await flushList(page([user()]));

    buttonLabelled('Edit')!.click();
    await render();

    const password = element.querySelector<HTMLInputElement>('#user-password');
    password!.value = 'newpassword';
    password!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    const patched = pendingRequests('PATCH').at(-1);
    expect(patched?.request.body).toEqual({
      username: 'noor',
      role: 'User',
      password: 'newpassword',
    });

    patched?.flush(user());
    await render();
    await flushList(page([user()]));
  });

  it('deletes only after the confirmation is accepted', async () => {
    await flushList(page([user()]));

    buttonLabelled('Delete')!.click();
    await render();

    // Asking is not deleting.
    expect(http.match((request) => request.method === 'DELETE')).toEqual([]);

    buttonLabelled('Delete user')!.click();
    await render();

    const removed = pendingRequests('DELETE').at(-1);
    expect(removed?.request.url).toBe('/api/users/u1');

    removed?.flush(null, { status: 204, statusText: 'No Content' });
    await render();
    await flushList(page([]));

    expect(element.querySelector('.empty__title')?.textContent).toContain('No users yet');
  });

  it('announces a delete that worked', async () => {
    await flushList(page([user()]));

    buttonLabelled('Delete')!.click();
    await render();
    buttonLabelled('Delete user')!.click();
    await render();

    pendingRequests('DELETE').at(-1)?.flush(null, { status: 204, statusText: 'No Content' });
    await render();
    await flushList(page([]));

    expect(toasts.toasts().map((toast) => [toast.kind, toast.message])).toEqual([
      ['success', 'User deleted successfully.'],
    ]);
  });

  it('reports a delete the API refused and leaves the row in place', async () => {
    await flushList(page([user()]));

    buttonLabelled('Delete')!.click();
    await render();
    buttonLabelled('Delete user')!.click();
    await render();

    // No body, so the canned sentence for a 403 is what the reader falls back to.
    pendingRequests('DELETE').at(-1)?.flush(null, { status: 403, statusText: 'Forbidden' });
    await render();

    // Reported once, by the interceptor - the row itself says nothing.
    expect(toasts.toasts().map((toast) => toast.kind)).toEqual(['error']);
    expect(toasts.toasts()[0].message).toContain('permission');
    expect(rowNames()).toEqual(['noor']);
  });

  it('leaves a failed listing to the page and does not also toast it', async () => {
    pendingRequests('GET').at(-1)?.flush('nope', { status: 500, statusText: 'Server Error' });
    await render();

    expect(element.querySelector('.notice')).not.toBeNull();
    expect(element.querySelector('.table')).toBeNull();
    expect(toasts.toasts()).toEqual([]);
  });
});
