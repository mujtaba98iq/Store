import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { errorInterceptor } from '@app/core/interceptors/error-interceptor';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { ToastService } from '@app/core/services/common/toast';
import { ProductVariant } from '../../models/product-variant.model';
import { ProductOption } from '../../models/product.model';
import { ProductVariants } from './product-variants';

function variant(overrides: Partial<ProductVariant> = {}): ProductVariant {
  return {
    id: 'v1',
    productId: 'p1',
    productName: 'Radiance Serum',
    sku: 'GLOW-SER-30',
    price: 48,
    barcode: '501234567890',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    createdById: 'u0',
    updatedById: null,
    ...overrides,
  };
}

function product(overrides: Partial<ProductOption> = {}): ProductOption {
  return { id: 'p1', name: 'Radiance Serum', ...overrides };
}

function page<T>(data: readonly T[], totalCount = data.length): PaginatedResult<T> {
  return { totalCount, data };
}

describe('ProductVariants', () => {
  let fixture: ComponentFixture<ProductVariants>;
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
      (request) => request.url.startsWith('/api/productvariants') && request.method === method,
    );
  };

  /**
   * The page also fetches the products behind the toolbar filter and the form's
   * picker, so that lookup is answered alongside the listing rather than left open
   * for `verify` to trip over.
   */
  const flushProducts = (products: readonly ProductOption[] = [product()]) => {
    fixture.detectChanges();
    for (const request of http.match((candidate) => candidate.url.startsWith('/api/products'))) {
      request.flush(page(products));
    }
  };

  const render = async () => {
    await Promise.resolve();
    fixture.detectChanges();
  };

  /** Answers the pending listing request and re-renders with the result. */
  const flushList = async (
    result: PaginatedResult<ProductVariant>,
    products: readonly ProductOption[] = [product()],
  ) => {
    flushProducts(products);
    const latest = pendingRequests('GET').at(-1);
    latest?.flush(result);
    await render();
    return latest;
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductVariants],
      providers: [
        // The page reports failures through the interceptor, so it is part of
        // what these tests exercise.
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    toasts = TestBed.inject(ToastService);
    fixture = TestBed.createComponent(ProductVariants);
    element = fixture.nativeElement as HTMLElement;
  });

  afterEach(() => {
    http.verify({ ignoreCancelled: true });
    toasts.clear();
  });

  it('lists the page the API returns', async () => {
    await flushList(page([variant(), variant({ id: 'v2', sku: 'ROSE-TON-50' })]));

    expect(rowSkus()).toEqual(['GLOW-SER-30', 'ROSE-TON-50']);
    expect(element.querySelector('.admin__count')?.textContent).toContain('2 variants');
  });

  it('asks the API for the first page ordered newest first', async () => {
    const request = await flushList(page([variant()]));

    expect(request?.request.params.get('Page')).toBe('1');
    expect(request?.request.params.get('PageSize')).toBe('10');
    expect(request?.request.params.get('OrderBy')).toBe('1');
    expect(request?.request.params.get('OrderByDirection')).toBe('1');
    expect(request?.request.params.has('Sku')).toBe(false);
    expect(request?.request.params.has('Barcode')).toBe(false);
    expect(request?.request.params.has('ProductId')).toBe(false);
    expect(request?.request.params.has('IsActive')).toBe(false);
  });

  it('sends the typed term as the Sku filter', async () => {
    await flushList(page([variant()]));

    const search = element.querySelector<HTMLInputElement>('#variant-search');
    search!.value = 'glow';
    search!.dispatchEvent(new Event('input'));

    // The term is debounced, so let the timer and the effect settle.
    await new Promise((resolve) => setTimeout(resolve, 350));
    const request = await flushList(page([variant()]));

    expect(request?.request.params.get('Sku')).toBe('glow');
  });

  it('sends the typed barcode as its own filter', async () => {
    await flushList(page([variant()]));

    const barcode = element.querySelector<HTMLInputElement>('#variant-barcode-filter');
    barcode!.value = '5012';
    barcode!.dispatchEvent(new Event('input'));

    await new Promise((resolve) => setTimeout(resolve, 350));
    const request = await flushList(page([variant()]));

    expect(request?.request.params.get('Barcode')).toBe('5012');
  });

  it('narrows to one product, and widens again', async () => {
    await flushList(page([variant()]), [product(), product({ id: 'p2', name: 'Rose Toner' })]);

    const picker = element.querySelector<HTMLSelectElement>('#variant-product-filter');
    picker!.value = 'p2';
    picker!.dispatchEvent(new Event('change'));

    const narrowed = await flushList(page([]));
    expect(narrowed?.request.params.get('ProductId')).toBe('p2');

    picker!.value = '';
    picker!.dispatchEvent(new Event('change'));

    const widened = await flushList(page([variant()]));
    expect(widened?.request.params.has('ProductId')).toBe(false);
  });

  it('sends the chosen status pill as the IsActive filter, and drops it again', async () => {
    await flushList(page([variant()]));

    buttonLabelled('Inactive')!.click();
    const inactive = await flushList(page([]));
    // False is a filter of its own, so it has to survive the round trip.
    expect(inactive?.request.params.get('IsActive')).toBe('false');

    buttonLabelled('Active')!.click();
    const active = await flushList(page([variant()]));
    expect(active?.request.params.get('IsActive')).toBe('true');

    buttonLabelled('All variants')!.click();
    const everything = await flushList(page([variant()]));
    expect(everything?.request.params.has('IsActive')).toBe(false);
  });

  it('translates the sort choice into OrderBy and OrderByDirection', async () => {
    await flushList(page([variant()]));

    const sort = element.querySelector<HTMLSelectElement>('#variant-sort');
    sort!.value = 'price-desc';
    sort!.dispatchEvent(new Event('change'));

    const request = await flushList(page([variant()]));
    expect(request?.request.params.get('OrderBy')).toBe('3');
    expect(request?.request.params.get('OrderByDirection')).toBe('1');
  });

  it('pages through the results', async () => {
    await flushList(page([variant()], 30));

    element.querySelector<HTMLButtonElement>('[aria-label="Next page"]')!.click();

    const request = await flushList(page([variant()], 30));
    expect(request?.request.params.get('Page')).toBe('2');
  });

  it('says a variant with no price of its own sells at the product price', async () => {
    await flushList(page([variant({ price: null })]));

    expect(element.querySelector('.table__inherited')?.textContent?.trim()).toBe('Product price');
  });

  it('creates a variant, and leaves the blank optional fields off the request', async () => {
    await flushList(page([variant()]), [product(), product({ id: 'p2', name: 'Rose Toner' })]);

    buttonLabelled('New variant')!.click();
    await render();

    const picker = element.querySelector<HTMLSelectElement>('#variant-product');
    picker!.value = 'p2';
    picker!.dispatchEvent(new Event('change'));

    const sku = element.querySelector<HTMLInputElement>('#variant-sku');
    sku!.value = 'ROSE-TON-50';
    sku!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    const posted = pendingRequests('POST').at(-1);
    // An empty price and barcode are omitted, not sent as null: the variant then
    // sells at the product's price and carries no barcode.
    expect(posted?.request.body).toEqual({
      productId: 'p2',
      sku: 'ROSE-TON-50',
      isActive: true,
    });

    posted?.flush(variant({ id: 'v2', productId: 'p2', sku: 'ROSE-TON-50', price: null }));
    await render();

    // The dialog closes and the listing refetches.
    expect(element.querySelector('app-product-variant-form')).toBeNull();
    await flushList(page([variant(), variant({ id: 'v2', sku: 'ROSE-TON-50' })]));
    expect(rowSkus()).toContain('ROSE-TON-50');
  });

  it('sends the price and barcode when they are filled in', async () => {
    await flushList(page([variant()]));

    buttonLabelled('New variant')!.click();
    await render();

    const picker = element.querySelector<HTMLSelectElement>('#variant-product');
    picker!.value = 'p1';
    picker!.dispatchEvent(new Event('change'));

    const sku = element.querySelector<HTMLInputElement>('#variant-sku');
    sku!.value = 'GLOW-SER-50';
    sku!.dispatchEvent(new Event('input'));

    const price = element.querySelector<HTMLInputElement>('#variant-price');
    price!.value = '62.5';
    price!.dispatchEvent(new Event('input'));

    const barcode = element.querySelector<HTMLInputElement>('#variant-barcode');
    barcode!.value = '501234567891';
    barcode!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    const posted = pendingRequests('POST').at(-1);
    expect(posted?.request.body).toEqual({
      productId: 'p1',
      sku: 'GLOW-SER-50',
      isActive: true,
      price: 62.5,
      barcode: '501234567891',
    });

    posted?.flush(variant({ id: 'v2', sku: 'GLOW-SER-50', price: 62.5 }));
    await render();
    await flushList(page([variant()]));
  });

  it('refuses a SKU shorter than the API would accept', async () => {
    await flushList(page([variant()]));

    buttonLabelled('New variant')!.click();
    await render();

    const picker = element.querySelector<HTMLSelectElement>('#variant-product');
    picker!.value = 'p1';
    picker!.dispatchEvent(new Event('change'));

    const sku = element.querySelector<HTMLInputElement>('#variant-sku');
    sku!.value = 'ab';
    sku!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    // Caught here rather than by the API, and the dialog stays open to be fixed.
    expect(http.match((request) => request.method === 'POST')).toEqual([]);
    expect(element.querySelector('app-product-variant-form')).not.toBeNull();
    expect(element.textContent).toContain('A SKU of 3 to 50 characters.');
  });

  it('sends an edit without the product, which is fixed for the variant', async () => {
    await flushList(page([variant()]));

    buttonLabelled('Edit')!.click();
    await render();

    // The product is settled, so the picker is not offered at all.
    expect(element.querySelector('#variant-product')).toBeNull();
    expect(element.querySelector<HTMLInputElement>('#variant-sku')?.value).toBe('GLOW-SER-30');

    const sku = element.querySelector<HTMLInputElement>('#variant-sku');
    sku!.value = 'GLOW-SER-31';
    sku!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    const patched = pendingRequests('PATCH').at(-1);
    expect(patched?.request.url).toBe('/api/productvariants/v1');
    expect(patched?.request.body).toEqual({
      sku: 'GLOW-SER-31',
      price: 48,
      barcode: '501234567890',
      isActive: true,
    });

    patched?.flush(variant({ sku: 'GLOW-SER-31' }));
    await render();
    await flushList(page([variant({ sku: 'GLOW-SER-31' })]));
  });

  it('deletes only after the confirmation is accepted', async () => {
    await flushList(page([variant()]));

    buttonLabelled('Delete')!.click();
    await render();

    // Asking is not deleting.
    expect(http.match((request) => request.method === 'DELETE')).toEqual([]);

    buttonLabelled('Delete variant')!.click();
    await render();

    const removed = pendingRequests('DELETE').at(-1);
    expect(removed?.request.url).toBe('/api/productvariants/v1');

    removed?.flush(null, { status: 204, statusText: 'No Content' });
    await render();
    await flushList(page([]));

    expect(element.querySelector('.empty__title')?.textContent).toContain(
      'No product variants yet',
    );
  });

  it('announces a delete that worked', async () => {
    await flushList(page([variant()]));

    buttonLabelled('Delete')!.click();
    await render();
    buttonLabelled('Delete variant')!.click();
    await render();

    pendingRequests('DELETE').at(-1)?.flush(null, { status: 204, statusText: 'No Content' });
    await render();
    await flushList(page([]));

    expect(toasts.toasts().map((toast) => [toast.kind, toast.message])).toEqual([
      ['success', 'Product variant deleted successfully.'],
    ]);
  });

  it('reports a variant the API refused to delete, and leaves the row in place', async () => {
    await flushList(page([variant()]));

    buttonLabelled('Delete')!.click();
    await render();
    buttonLabelled('Delete variant')!.click();
    await render();

    pendingRequests('DELETE')
      .at(-1)
      ?.flush(
        { message: 'Product variant GLOW-SER-30 still has stock. Delete its stock row first.' },
        { status: 409, statusText: 'Conflict' },
      );
    await render();

    // Reported once, by the interceptor - the row itself says nothing.
    expect(toasts.toasts().map((toast) => toast.kind)).toEqual(['error']);
    expect(toasts.toasts()[0].message).toContain('still has stock');
    expect(rowSkus()).toEqual(['GLOW-SER-30']);
  });

  it('reports a SKU another variant already holds, and keeps the dialog open', async () => {
    await flushList(page([variant()]));

    buttonLabelled('New variant')!.click();
    await render();

    const picker = element.querySelector<HTMLSelectElement>('#variant-product');
    picker!.value = 'p1';
    picker!.dispatchEvent(new Event('change'));

    const sku = element.querySelector<HTMLInputElement>('#variant-sku');
    sku!.value = 'GLOW-SER-30';
    sku!.dispatchEvent(new Event('input'));

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    await render();

    pendingRequests('POST')
      .at(-1)
      ?.flush(
        { message: 'Product variant with SKU GLOW-SER-30 already exists' },
        { status: 409, statusText: 'Conflict' },
      );
    await render();

    // Not a failure about one field, so the interceptor is what reports it.
    expect(toasts.toasts()[0].message).toContain('already exists');
    expect(element.querySelector('app-product-variant-form')).not.toBeNull();
  });

  it('leaves a failed listing to the page and does not also toast it', async () => {
    flushProducts();
    pendingRequests('GET').at(-1)?.flush('nope', { status: 500, statusText: 'Server Error' });
    await render();

    expect(element.querySelector('.notice')).not.toBeNull();
    expect(element.querySelector('.table')).toBeNull();
    expect(toasts.toasts()).toEqual([]);
  });
});
