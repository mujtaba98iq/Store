import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { errorInterceptor } from '@app/core/interceptors/error-interceptor';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { CategoryDetail } from '@app/core/models/interfaces/category';
import { ToastService } from '@app/core/services/common/toast';
import { Categories } from './categories';

function category(overrides: Partial<CategoryDetail> = {}): CategoryDetail {
  return {
    id: 'c1',
    name: 'Moisturisers',
    description: 'Barrier creams and lotions.',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    createdById: 'u1',
    updatedById: null,
    ...overrides,
  };
}

function page<T>(data: readonly T[], totalCount = data.length): PaginatedResult<T> {
  return { totalCount, data };
}

describe('Categories', () => {
  let fixture: ComponentFixture<Categories>;
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
      (request) => request.url.startsWith('/api/categories') && request.method === method,
    );
  };

  const render = async () => {
    await Promise.resolve();
    fixture.detectChanges();
  };

  /** Answers the pending listing request and re-renders with the result. */
  const flushList = async (result: PaginatedResult<CategoryDetail>) => {
    const latest = pendingRequests('GET').at(-1);
    latest?.flush(result);
    await render();
    return latest;
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Categories],
      providers: [
        // The page reports failures through the interceptor, so it is part of
        // what these tests exercise.
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    toasts = TestBed.inject(ToastService);
    fixture = TestBed.createComponent(Categories);
    element = fixture.nativeElement as HTMLElement;
  });

  afterEach(() => {
    http.verify({ ignoreCancelled: true });
    toasts.clear();
  });

  it('lists the page the API returns', async () => {
    await flushList(page([category(), category({ id: 'c2', name: 'Serums' })]));

    expect(rowNames()).toEqual(['Moisturisers', 'Serums']);
    expect(element.querySelector('.admin__count')?.textContent).toContain('2 categories');
  });

  it('asks the API for the first page ordered newest first', async () => {
    const request = await flushList(page([category()]));

    expect(request?.request.params.get('Page')).toBe('1');
    expect(request?.request.params.get('PageSize')).toBe('10');
    expect(request?.request.params.get('OrderBy')).toBe('1');
    expect(request?.request.params.get('OrderByDirection')).toBe('1');
    expect(request?.request.params.has('Name')).toBe(false);
    expect(request?.request.params.has('Description')).toBe(false);
  });

  it('sends the typed term as the filter the chosen pill names', async () => {
    await flushList(page([category()]));

    const search = element.querySelector<HTMLInputElement>('#category-search');
    search!.value = 'serum';
    search!.dispatchEvent(new Event('input'));

    // The term is debounced, so let the timer and the effect settle.
    await new Promise((resolve) => setTimeout(resolve, 350));
    const byName = await flushList(page([category()]));
    expect(byName?.request.params.get('Name')).toBe('serum');
    expect(byName?.request.params.has('Description')).toBe(false);

    buttonLabelled('Description')!.click();

    const byDescription = await flushList(page([category()]));
    expect(byDescription?.request.params.get('Description')).toBe('serum');
    expect(byDescription?.request.params.has('Name')).toBe(false);
  });

  it('translates the sort choice into OrderBy and OrderByDirection', async () => {
    await flushList(page([category()]));

    const sort = element.querySelector<HTMLSelectElement>('#category-sort');
    sort!.value = 'name-asc';
    sort!.dispatchEvent(new Event('change'));

    const request = await flushList(page([category()]));
    expect(request?.request.params.get('OrderBy')).toBe('2');
    expect(request?.request.params.get('OrderByDirection')).toBe('0');
  });

  it('pages through the results', async () => {
    await flushList(page([category()], 30));

    buttonLabelled('Next')!.click();

    const request = await flushList(page([category()], 30));
    expect(request?.request.params.get('Page')).toBe('2');
  });

  it('creates a category from the form', async () => {
    await flushList(page([category()]));

    buttonLabelled('New category')!.click();
    await render();

    const name = element.querySelector<HTMLInputElement>('#category-name');
    name!.value = 'Cleansers';
    name!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    const posted = pendingRequests('POST').at(-1);
    expect(posted?.request.body).toEqual({ name: 'Cleansers', description: null });

    posted?.flush(category({ id: 'c3', name: 'Cleansers' }));
    await render();

    // The dialog closes and the listing refetches.
    expect(element.querySelector('app-category-form')).toBeNull();
    await flushList(page([category(), category({ id: 'c3', name: 'Cleansers' })]));
    expect(rowNames()).toContain('Cleansers');
  });

  it('patches only what the edit form changed', async () => {
    await flushList(page([category()]));

    buttonLabelled('Edit')!.click();
    await render();

    const name = element.querySelector<HTMLInputElement>('#category-name');
    expect(name?.value).toBe('Moisturisers');
    name!.value = 'Moisturizers';
    name!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    const patched = pendingRequests('PATCH').at(-1);
    expect(patched?.request.url).toBe('/api/categories/c1');
    expect(patched?.request.body).toEqual({
      name: 'Moisturizers',
      description: 'Barrier creams and lotions.',
    });

    patched?.flush(category({ name: 'Moisturizers' }));
    await render();
    await flushList(page([category({ name: 'Moisturizers' })]));
  });

  it('deletes only after the confirmation is accepted', async () => {
    await flushList(page([category()]));

    buttonLabelled('Delete')!.click();
    await render();

    // Asking is not deleting.
    expect(http.match((request) => request.method === 'DELETE')).toEqual([]);

    buttonLabelled('Delete category')!.click();
    await render();

    const removed = pendingRequests('DELETE').at(-1);
    expect(removed?.request.url).toBe('/api/categories/c1');

    removed?.flush(null, { status: 204, statusText: 'No Content' });
    await render();
    await flushList(page([]));

    expect(element.querySelector('.empty__title')?.textContent).toContain('No categories yet');
  });

  it('announces a delete that worked', async () => {
    await flushList(page([category()]));

    buttonLabelled('Delete')!.click();
    await render();
    buttonLabelled('Delete category')!.click();
    await render();

    pendingRequests('DELETE').at(-1)?.flush(null, { status: 204, statusText: 'No Content' });
    await render();
    await flushList(page([]));

    expect(toasts.toasts().map((toast) => [toast.kind, toast.message])).toEqual([
      ['success', 'Category deleted successfully.'],
    ]);
  });

  it('reports a delete the API refused and leaves the row in place', async () => {
    await flushList(page([category()]));

    buttonLabelled('Delete')!.click();
    await render();
    buttonLabelled('Delete category')!.click();
    await render();

    // No body, so the canned sentence for a 403 is what the reader falls back to.
    pendingRequests('DELETE').at(-1)?.flush(null, { status: 403, statusText: 'Forbidden' });
    await render();

    // Reported once, by the interceptor - the row itself says nothing.
    expect(toasts.toasts().map((toast) => toast.kind)).toEqual(['error']);
    expect(toasts.toasts()[0].message).toContain('permission');
    expect(rowNames()).toEqual(['Moisturisers']);
  });

  it('leaves a failed listing to the page and does not also toast it', async () => {
    pendingRequests('GET').at(-1)?.flush('nope', { status: 500, statusText: 'Server Error' });
    await render();

    expect(element.querySelector('.notice')).not.toBeNull();
    expect(toasts.toasts()).toEqual([]);
  });

  it('surfaces a failed listing with a retry', async () => {
    pendingRequests('GET').at(-1)?.flush('nope', { status: 500, statusText: 'Server Error' });
    await render();

    expect(element.querySelector('.notice')).not.toBeNull();
    expect(element.querySelector('.table')).toBeNull();
  });
});
