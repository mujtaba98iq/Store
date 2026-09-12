import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { errorInterceptor } from '@app/core/interceptors/error-interceptor';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { ToastService } from '@app/core/services/common/toast';
import { Inventory } from '../../models/inventory.model';
import { ProductVariant } from '../../models/product-variant.model';
import { Inventories } from './inventories';

function inventory(overrides: Partial<Inventory> = {}): Inventory {
  return {
    id: 'i1',
    productVariantId: 'v1',
    sku: 'GLOW-SER-30',
    quantity: 120,
    reservedQuantity: 12,
    availableQuantity: 108,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    createdById: 'u0',
    updatedById: null,
    ...overrides,
  };
}

function variant(overrides: Partial<ProductVariant> = {}): ProductVariant {
  return { id: 'v1', sku: 'GLOW-SER-30', isActive: true, ...overrides };
}

function page<T>(data: readonly T[], totalCount = data.length): PaginatedResult<T> {
  return { totalCount, data };
}

describe('Inventories', () => {
  let fixture: ComponentFixture<Inventories>;
  let element: HTMLElement;
  let http: HttpTestingController;
  let toasts: ToastService;

  const rowSkus = () =>
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
      (request) => request.url.startsWith('/api/inventories') && request.method === method,
    );
  };

  /**
   * The page also fetches the variants behind the form's picker - on open, and
   * again after every save or delete - so that lookup is answered alongside the
   * listing rather than left open for `verify` to trip over.
   */
  const flushVariants = (variants: readonly ProductVariant[] = [variant()]) => {
    fixture.detectChanges();
    for (const request of http.match((candidate) =>
      candidate.url.startsWith('/api/productvariants'),
    )) {
      request.flush(page(variants));
    }
  };

  const render = async () => {
    await Promise.resolve();
    fixture.detectChanges();
  };

  /** Answers the pending listing request and re-renders with the result. */
  const flushList = async (
    result: PaginatedResult<Inventory>,
    variants: readonly ProductVariant[] = [variant()],
  ) => {
    flushVariants(variants);
    const latest = pendingRequests('GET').at(-1);
    latest?.flush(result);
    await render();
    return latest;
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Inventories],
      providers: [
        // The page reports failures through the interceptor, so it is part of
        // what these tests exercise.
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    toasts = TestBed.inject(ToastService);
    fixture = TestBed.createComponent(Inventories);
    element = fixture.nativeElement as HTMLElement;
  });

  afterEach(() => {
    http.verify({ ignoreCancelled: true });
    toasts.clear();
  });

  it('lists the page the API returns', async () => {
    await flushList(
      page([inventory(), inventory({ id: 'i2', productVariantId: 'v2', sku: 'ROSE-TON-50' })]),
    );

    expect(rowSkus()).toEqual(['GLOW-SER-30', 'ROSE-TON-50']);
    expect(element.querySelector('.admin__count')?.textContent).toContain('2 stock rows');
  });

  it('asks the API for the first page ordered newest first', async () => {
    const request = await flushList(page([inventory()]));

    expect(request?.request.params.get('Page')).toBe('1');
    expect(request?.request.params.get('PageSize')).toBe('10');
    expect(request?.request.params.get('OrderBy')).toBe('1');
    expect(request?.request.params.get('OrderByDirection')).toBe('1');
    expect(request?.request.params.has('Sku')).toBe(false);
    expect(request?.request.params.has('IsAvailable')).toBe(false);
    expect(request?.request.params.has('MinQuantity')).toBe(false);
    expect(request?.request.params.has('MaxQuantity')).toBe(false);
  });

  it('sends the typed term as the Sku filter', async () => {
    await flushList(page([inventory()]));

    const search = element.querySelector<HTMLInputElement>('#inventory-search');
    search!.value = 'glow';
    search!.dispatchEvent(new Event('input'));

    // The term is debounced, so let the timer and the effect settle.
    await new Promise((resolve) => setTimeout(resolve, 350));
    const request = await flushList(page([inventory()]));

    expect(request?.request.params.get('Sku')).toBe('glow');
  });

  it('sends the quantity bounds, and leaves a blank box off the request', async () => {
    await flushList(page([inventory()]));

    const min = element.querySelector<HTMLInputElement>('#inventory-min');
    min!.value = '0';
    min!.dispatchEvent(new Event('input'));

    const max = element.querySelector<HTMLInputElement>('#inventory-max');
    max!.value = '50';
    max!.dispatchEvent(new Event('input'));

    await new Promise((resolve) => setTimeout(resolve, 350));
    const request = await flushList(page([inventory()]));

    // Zero is a real floor, not an empty box, so it is sent rather than dropped.
    expect(request?.request.params.get('MinQuantity')).toBe('0');
    expect(request?.request.params.get('MaxQuantity')).toBe('50');
  });

  it('sends the chosen availability pill as the IsAvailable filter, and drops it again', async () => {
    await flushList(page([inventory()]));

    buttonLabelled('Sold out')!.click();
    const soldOut = await flushList(page([]));
    // False is a filter of its own, so it has to survive the round trip.
    expect(soldOut?.request.params.get('IsAvailable')).toBe('false');

    buttonLabelled('In stock')!.click();
    const inStock = await flushList(page([inventory()]));
    expect(inStock?.request.params.get('IsAvailable')).toBe('true');

    buttonLabelled('All stock')!.click();
    const everything = await flushList(page([inventory()]));
    expect(everything?.request.params.has('IsAvailable')).toBe(false);
  });

  it('translates the sort choice into OrderBy and OrderByDirection', async () => {
    await flushList(page([inventory()]));

    const sort = element.querySelector<HTMLSelectElement>('#inventory-sort');
    sort!.value = 'sku-asc';
    sort!.dispatchEvent(new Event('change'));

    const request = await flushList(page([inventory()]));
    expect(request?.request.params.get('OrderBy')).toBe('6');
    expect(request?.request.params.get('OrderByDirection')).toBe('0');
  });

  it('pages through the results', async () => {
    await flushList(page([inventory()], 30));

    element.querySelector<HTMLButtonElement>('[aria-label="Next page"]')!.click();

    const request = await flushList(page([inventory()], 30));
    expect(request?.request.params.get('Page')).toBe('2');
  });

  it('marks a row with nothing left to sell', async () => {
    await flushList(
      page([inventory({ quantity: 40, reservedQuantity: 40, availableQuantity: 0 })]),
    );

    expect(element.querySelector('.stock--out')?.textContent?.trim()).toBe('Sold out');
  });

  it('creates a stock row from the form', async () => {
    await flushList(page([inventory()]), [variant({ id: 'v2', sku: 'ROSE-TON-50' })]);

    buttonLabelled('New stock row')!.click();
    await render();

    const picker = element.querySelector<HTMLSelectElement>('#inventory-variant');
    picker!.value = 'v2';
    picker!.dispatchEvent(new Event('change'));

    const quantity = element.querySelector<HTMLInputElement>('#inventory-quantity');
    quantity!.value = '40';
    quantity!.dispatchEvent(new Event('input'));

    const reserved = element.querySelector<HTMLInputElement>('#inventory-reserved');
    reserved!.value = '5';
    reserved!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    const posted = pendingRequests('POST').at(-1);
    expect(posted?.request.body).toEqual({
      productVariantId: 'v2',
      quantity: 40,
      reservedQuantity: 5,
    });

    posted?.flush(
      inventory({
        id: 'i2',
        productVariantId: 'v2',
        sku: 'ROSE-TON-50',
        quantity: 40,
        reservedQuantity: 5,
        availableQuantity: 35,
      }),
    );
    await render();

    // The dialog closes and the listing refetches.
    expect(element.querySelector('app-inventory-form')).toBeNull();
    await flushList(
      page([inventory(), inventory({ id: 'i2', productVariantId: 'v2', sku: 'ROSE-TON-50' })]),
    );
    expect(rowSkus()).toContain('ROSE-TON-50');
  });

  it('shows what is left to sell as the counts are typed', async () => {
    await flushList(page([inventory()]));

    buttonLabelled('New stock row')!.click();
    await render();

    const quantity = element.querySelector<HTMLInputElement>('#inventory-quantity');
    quantity!.value = '40';
    quantity!.dispatchEvent(new Event('input'));

    const reserved = element.querySelector<HTMLInputElement>('#inventory-reserved');
    reserved!.value = '15';
    reserved!.dispatchEvent(new Event('input'));
    await render();

    expect(element.querySelector('.form__preview')?.textContent).toContain('25');
  });

  it('refuses to send more reserved than is on hand', async () => {
    await flushList(page([inventory()]));

    buttonLabelled('New stock row')!.click();
    await render();

    const picker = element.querySelector<HTMLSelectElement>('#inventory-variant');
    picker!.value = 'v1';
    picker!.dispatchEvent(new Event('change'));

    const quantity = element.querySelector<HTMLInputElement>('#inventory-quantity');
    quantity!.value = '5';
    quantity!.dispatchEvent(new Event('input'));

    const reserved = element.querySelector<HTMLInputElement>('#inventory-reserved');
    reserved!.value = '9';
    reserved!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    // Caught here rather than by the API, and the dialog stays open to be fixed.
    expect(http.match((request) => request.method === 'POST')).toEqual([]);
    expect(element.querySelector('app-inventory-form')).not.toBeNull();
    expect(element.textContent).toContain('Reserved cannot be more than what is on hand.');
  });

  it('sends only the counts on an edit, and keeps the variant out of it', async () => {
    await flushList(page([inventory()]));

    buttonLabelled('Edit')!.click();
    await render();

    // The variant is settled, so the picker is not offered at all.
    expect(element.querySelector('#inventory-variant')).toBeNull();
    expect(element.querySelector<HTMLInputElement>('#inventory-quantity')?.value).toBe('120');

    const quantity = element.querySelector<HTMLInputElement>('#inventory-quantity');
    quantity!.value = '150';
    quantity!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    const patched = pendingRequests('PATCH').at(-1);
    expect(patched?.request.url).toBe('/api/inventories/i1');
    expect(patched?.request.body).toEqual({ quantity: 150, reservedQuantity: 12 });

    patched?.flush(inventory({ quantity: 150, availableQuantity: 138 }));
    await render();
    await flushList(page([inventory({ quantity: 150, availableQuantity: 138 })]));
  });

  it('deletes only after the confirmation is accepted', async () => {
    await flushList(page([inventory()]));

    buttonLabelled('Delete')!.click();
    await render();

    // Asking is not deleting.
    expect(http.match((request) => request.method === 'DELETE')).toEqual([]);

    buttonLabelled('Delete stock row')!.click();
    await render();

    const removed = pendingRequests('DELETE').at(-1);
    expect(removed?.request.url).toBe('/api/inventories/i1');

    removed?.flush(null, { status: 204, statusText: 'No Content' });
    await render();
    await flushList(page([]));

    expect(element.querySelector('.empty__title')?.textContent).toContain('No stock rows yet');
  });

  it('announces a delete that worked', async () => {
    await flushList(page([inventory()]));

    buttonLabelled('Delete')!.click();
    await render();
    buttonLabelled('Delete stock row')!.click();
    await render();

    pendingRequests('DELETE').at(-1)?.flush(null, { status: 204, statusText: 'No Content' });
    await render();
    await flushList(page([]));

    expect(toasts.toasts().map((toast) => [toast.kind, toast.message])).toEqual([
      ['success', 'Stock row deleted successfully.'],
    ]);
  });

  it('reports a delete the API refused and leaves the row in place', async () => {
    await flushList(page([inventory()]));

    buttonLabelled('Delete')!.click();
    await render();
    buttonLabelled('Delete stock row')!.click();
    await render();

    // No body, so the canned sentence for a 403 is what the reader falls back to.
    pendingRequests('DELETE').at(-1)?.flush(null, { status: 403, statusText: 'Forbidden' });
    await render();

    // Reported once, by the interceptor - the row itself says nothing.
    expect(toasts.toasts().map((toast) => toast.kind)).toEqual(['error']);
    expect(toasts.toasts()[0].message).toContain('permission');
    expect(rowSkus()).toEqual(['GLOW-SER-30']);
  });

  it('reports a variant that is already stocked, and keeps the dialog open', async () => {
    await flushList(page([inventory()]));

    buttonLabelled('New stock row')!.click();
    await render();

    const picker = element.querySelector<HTMLSelectElement>('#inventory-variant');
    picker!.value = 'v1';
    picker!.dispatchEvent(new Event('change'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    pendingRequests('POST')
      .at(-1)
      ?.flush(
        { message: 'Inventory with SKU GLOW-SER-30 already exists' },
        { status: 409, statusText: 'Conflict' },
      );
    await render();

    // Not a failure about one field, so the interceptor is what reports it.
    expect(toasts.toasts()[0].message).toContain('already exists');
    expect(element.querySelector('app-inventory-form')).not.toBeNull();
  });

  it('leaves a failed listing to the page and does not also toast it', async () => {
    flushVariants();
    pendingRequests('GET').at(-1)?.flush('nope', { status: 500, statusText: 'Server Error' });
    await render();

    expect(element.querySelector('.notice')).not.toBeNull();
    expect(element.querySelector('.table')).toBeNull();
    expect(toasts.toasts()).toEqual([]);
  });
});
