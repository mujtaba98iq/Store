import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Category, PaginatedResult, Product } from './product';
import { Products } from './products';

const MOISTURISERS: Category = { id: 'c1', name: 'Moisturisers', description: null };

function product(overrides: Partial<Product> = {}): Product {
  return {
    id: 'p1',
    name: 'Moisture Repair Cream',
    description: 'Rich barrier cream.',
    price: 67,
    quantity: 4,
    imagePath: '/product-images/product-cream.svg',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    categories: [MOISTURISERS],
    images: [],
    ...overrides,
  };
}

function page<T>(data: readonly T[], totalCount = data.length): PaginatedResult<T> {
  return { totalCount, data };
}

describe('Products', () => {
  let fixture: ComponentFixture<Products>;
  let element: HTMLElement;
  let http: HttpTestingController;

  const cardNames = () =>
    Array.from(element.querySelectorAll('.card__name')).map((node) => node.textContent?.trim());

  /**
   * Resource requests are issued from effects, so change detection has to run -
   * but never await stability first: an open request keeps the app unstable.
   */
  const pendingRequests = (url: string) => {
    fixture.detectChanges();
    return http.match((request) => request.url === url);
  };

  const render = async () => {
    await Promise.resolve();
    fixture.detectChanges();
  };

  /** Answers the pending catalogue request and re-renders with the result. */
  const flushProducts = async (result: PaginatedResult<Product>) => {
    const latest = pendingRequests('/api/products').at(-1);
    latest?.flush(result);
    await render();
    return latest;
  };

  const flushCategories = async (categories: readonly Category[] = [MOISTURISERS]) => {
    pendingRequests('/api/categories').forEach((request) => request.flush(page(categories)));
    await render();
  };
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Products],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Products);
    element = fixture.nativeElement as HTMLElement;
    await flushCategories();
  });

  afterEach(() => {
    http.verify({ ignoreCancelled: true });
  });

  it('renders the page the API returns', async () => {
    await flushProducts(page([product(), product({ id: 'p2', name: 'Radiance Renewal Serum' })]));

    expect(cardNames()).toEqual(['Moisture Repair Cream', 'Radiance Renewal Serum']);
    expect(element.querySelector('.catalog__count')?.textContent).toContain('2 products');
  });

  it('asks the API for the first page ordered newest first', async () => {
    const request = await flushProducts(page([product()]));

    expect(request?.request.params.get('Page')).toBe('1');
    expect(request?.request.params.get('PageSize')).toBe('12');
    expect(request?.request.params.get('OrderBy')).toBe('1');
    expect(request?.request.params.get('OrderByDirection')).toBe('1');
    expect(request?.request.params.has('Name')).toBe(false);
  });

  it('builds the category pills from the categories endpoint', async () => {
    await flushProducts(page([product()]));

    const labels = Array.from(element.querySelectorAll('.pill')).map((pill) =>
      pill.textContent?.trim(),
    );
    expect(labels).toEqual(['All', 'Moisturisers']);
  });

  it('sends the chosen category to the API instead of filtering in the browser', async () => {
    await flushProducts(page([product()]));

    const pill = Array.from(element.querySelectorAll<HTMLButtonElement>('.pill')).find(
      (button) => button.textContent?.trim() === 'Moisturisers',
    );
    pill!.click();

    const request = await flushProducts(page([product()]));
    expect(request?.request.params.get('CategoryId')).toBe('c1');
  });

  it('translates the sort choice into OrderBy and OrderByDirection', async () => {
    await flushProducts(page([product()]));

    const sort = element.querySelector<HTMLSelectElement>('#product-sort');
    sort!.value = 'price-asc';
    sort!.dispatchEvent(new Event('change'));

    const request = await flushProducts(page([product()]));
    expect(request?.request.params.get('OrderBy')).toBe('3');
    expect(request?.request.params.get('OrderByDirection')).toBe('0');
  });

  it('pages through the results', async () => {
    await flushProducts(page([product()], 30));

    const next = Array.from(element.querySelectorAll<HTMLButtonElement>('.pager .btn')).find(
      (button) => button.textContent?.trim() === 'Next',
    );
    next!.click();

    const request = await flushProducts(page([product()], 30));
    expect(request?.request.params.get('Page')).toBe('2');
  });

  it('surfaces a failed request with a retry', async () => {
    const requests = pendingRequests('/api/products');
    requests.at(-1)?.flush('nope', { status: 500, statusText: 'Server Error' });
    await render();

    expect(element.querySelector('.notice')).not.toBeNull();
    expect(element.querySelector('.grid')).toBeNull();
  });

  it('hides the admin controls from a signed-out visitor', async () => {
    await flushProducts(page([product()]));

    expect(element.querySelector('.catalog__new')).toBeNull();
    expect(element.querySelector('.card__edit')).toBeNull();
  });
});
