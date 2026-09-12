import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { Cart } from '@app/core/models/interfaces/cart';
import { Category } from '@app/core/models/interfaces/category';
import { AuthStore } from '@app/core/services/common/auth-store';
import { ToastService } from '@app/core/services/common/toast';
import { Product } from '../../models/product.model';
import { ProductVariant } from '../../models/product-variant.model';
import { ProductDetails } from './product-details';

const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
const SERUMS: Category = { id: 'c1', name: 'Serums', description: null };

/** An unsigned token - the client only ever reads the claims. */
function tokenFor(email: string, role = 'User'): string {
  const payload = btoa(JSON.stringify({ [EMAIL_CLAIM]: email, role, exp: 4102444800 }))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');
  return `header.${payload}.signature`;
}

function variant(overrides: Partial<ProductVariant> = {}): ProductVariant {
  return {
    id: 'v1',
    productId: 'p1',
    productName: 'Radiance Renewal Serum',
    sku: 'GLOW-SER-30',
    price: null,
    barcode: null,
    isActive: true,
    ...overrides,
  };
}

function product(overrides: Partial<Product> = {}): Product {
  return {
    id: 'p1',
    name: 'Radiance Renewal Serum',
    description: 'A nightly vitamin C serum.',
    price: 48,
    quantity: 10,
    imagePath: '',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    categories: [SERUMS],
    variants: [variant()],
    images: [],
    ...overrides,
  };
}

function emptyCart(): Cart {
  return {
    id: 'c1',
    userId: 'u1',
    items: [],
    itemCount: 0,
    totalAmount: 0,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    createdById: 'u1',
    updatedById: null,
  };
}

describe('ProductDetails', () => {
  let fixture: ComponentFixture<ProductDetails>;
  let element: HTMLElement;
  let http: HttpTestingController;
  let toasts: ToastService;

  const variantLabels = () =>
    Array.from(element.querySelectorAll('.pill__sku')).map((node) => node.textContent?.trim());

  const selectedVariant = () =>
    element.querySelector('.pill--active .pill__sku')?.textContent?.trim();

  const buttonLabelled = (label: string) =>
    Array.from(element.querySelectorAll<HTMLButtonElement>('button')).find(
      (button) => button.textContent?.trim() === label,
    );

  const render = async () => {
    await Promise.resolve();
    fixture.detectChanges();
  };

  /**
   * Resource requests are issued from effects, so change detection has to run -
   * but never await stability first: an open request keeps the app unstable.
   */
  const pendingRequests = (url: string, method?: string) => {
    fixture.detectChanges();
    return http.match(
      (request) => request.url.startsWith(url) && (!method || request.method === method),
    );
  };

  /**
   * Signing in starts the shared cart fetch behind the header badge, which has
   * nothing to do with this page but is still an open request.
   */
  const flushOwnCart = () => {
    http
      .match((request) => request.url === '/api/carts/me' && request.method === 'GET')
      .forEach((request) => request.flush(emptyCart()));
  };

  const flushProduct = async (result = product()) => {
    const latest = pendingRequests('/api/products/p1', 'GET').at(-1);
    latest?.flush(result);
    await render();
    return latest;
  };

  /** Builds the page, signed in unless the test is about a visitor without a token. */
  const start = (signedIn = true) => {
    if (signedIn) {
      TestBed.inject(AuthStore).set({ accessToken: tokenFor('a@b.c'), refreshToken: 'r1' });
    }

    fixture = TestBed.createComponent(ProductDetails);
    element = fixture.nativeElement as HTMLElement;
  };

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [ProductDetails],
      providers: [
        provideRouter([]),
        {
          // The page reads the id from the parameter map rather than a snapshot,
          // so that is what the test has to hand it.
          provide: ActivatedRoute,
          useValue: { paramMap: of(convertToParamMap({ id: 'p1' })) },
        },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    toasts = TestBed.inject(ToastService);
  });

  afterEach(() => {
    flushOwnCart();
    http.verify({ ignoreCancelled: true });
    toasts.clear();
    localStorage.clear();
  });

  it('renders the product the route names', async () => {
    start();
    await flushProduct();

    expect(element.querySelector('.detail__title')?.textContent?.trim()).toBe(
      'Radiance Renewal Serum',
    );
    expect(element.querySelector('.detail__eyebrow')?.textContent?.trim()).toBe('Serums');
    expect(element.querySelector('.detail__copy')?.textContent).toContain('vitamin C');
  });

  it('takes the variants off the product rather than asking for them separately', async () => {
    start();
    await flushProduct();

    expect(variantLabels()).toEqual(['GLOW-SER-30']);
    // The variants endpoint is behind a role the shop does not need here.
    expect(http.match((request) => request.url.startsWith('/api/productvariants')).length).toBe(0);
  });

  it('offers the variants and picks the first, so the page is buyable at once', async () => {
    start();
    await flushProduct(
      product({
        variants: [variant(), variant({ id: 'v2', sku: 'GLOW-SER-50', price: 72 })],
      }),
    );

    expect(variantLabels()).toEqual(['GLOW-SER-30', 'GLOW-SER-50']);
    expect(selectedVariant()).toBe('GLOW-SER-30');
  });

  it('never offers a variant the API would refuse to cart', async () => {
    start();
    await flushProduct(
      product({
        variants: [variant({ isActive: false }), variant({ id: 'v2', sku: 'GLOW-SER-50' })],
      }),
    );

    expect(variantLabels()).toEqual(['GLOW-SER-50']);
    expect(selectedVariant()).toBe('GLOW-SER-50');
  });

  it('falls back to the product price for a variant that carries none', async () => {
    start();
    await flushProduct(product({ variants: [variant({ price: null })] }));

    expect(element.querySelector('.detail__price')?.textContent).toContain('48');
  });

  it("prefers the variant's own price once one is chosen", async () => {
    start();
    await flushProduct(
      product({
        variants: [variant(), variant({ id: 'v2', sku: 'GLOW-SER-50', price: 72 })],
      }),
    );

    Array.from(element.querySelectorAll<HTMLButtonElement>('.pill'))[1].click();
    await render();

    expect(element.querySelector('.detail__price')?.textContent).toContain('72');
  });

  it('posts the chosen variant and quantity to the cart', async () => {
    start();
    await flushProduct();
    flushOwnCart();

    buttonLabelled('+')!.click();
    await render();
    buttonLabelled('Add to Cart')!.click();

    const request = pendingRequests('/api/carts/me/items', 'POST').at(-1);
    expect(request?.request.body).toEqual({ productVariantId: 'v1', quantity: 2 });
    request?.flush(emptyCart());
    await render();
  });

  it('starts the count again when the shopper switches variant', async () => {
    start();
    await flushProduct(
      product({ variants: [variant(), variant({ id: 'v2', sku: 'GLOW-SER-50' })] }),
    );

    buttonLabelled('+')!.click();
    await render();
    expect(element.querySelector<HTMLInputElement>('#quantity')!.value).toBe('2');

    Array.from(element.querySelectorAll<HTMLButtonElement>('.pill'))[1].click();
    await render();

    expect(element.querySelector<HTMLInputElement>('#quantity')!.value).toBe('1');
  });

  it('never steps the count below one, which the API refuses', async () => {
    start();
    await flushProduct();

    expect(buttonLabelled('−')?.disabled).toBe(true);
  });

  it('says so when the product has no variant anyone can buy', async () => {
    start();
    await flushProduct(product({ variants: [] }));

    expect(element.querySelector('.buy__status')?.textContent).toContain('not on sale yet');
    expect(buttonLabelled('Add to Cart')).toBeUndefined();
  });

  it('reports a product that is no longer there rather than an empty page', async () => {
    start();
    pendingRequests('/api/products/p1', 'GET')
      .at(-1)
      ?.flush('Gone', { status: 404, statusText: 'Not Found' });
    await render();

    expect(element.querySelector('.notice__title')?.textContent).toBeTruthy();
  });

  it('shows a visitor the sizes, and sends them to sign in rather than to the API', async () => {
    start(false);
    await flushProduct();

    expect(variantLabels()).toEqual(['GLOW-SER-30']);
    expect(buttonLabelled('Add to Cart')).toBeUndefined();

    const signIn = element.querySelector<HTMLAnchorElement>('a.buy__add');
    expect(signIn?.textContent?.trim()).toBe('Sign in to add to cart');
    expect(signIn?.getAttribute('href')).toBe('/login?returnUrl=%2Fproducts%2Fp1');
  });

  it('never asks a signed-out visitor for their cart', async () => {
    start(false);
    await flushProduct();

    expect(http.match((request) => request.url.startsWith('/api/carts')).length).toBe(0);
  });
});
