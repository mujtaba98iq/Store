import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { errorInterceptor } from '@app/core/interceptors/error-interceptor';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { Cart as CartModel, CartItem } from '@app/core/models/interfaces/cart';
import { AuthStore } from '@app/core/services/common/auth-store';
import { ToastService } from '@app/core/services/common/toast';
import { ProductVariant } from '../../models/product-variant.model';
import { Cart } from './cart';

const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';

/** An unsigned token - the client only ever reads the claims. */
function tokenFor(email: string, role = 'User'): string {
  const payload = btoa(JSON.stringify({ [EMAIL_CLAIM]: email, role, exp: 4102444800 }))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');
  return `header.${payload}.signature`;
}

function item(overrides: Partial<CartItem> = {}): CartItem {
  const quantity = overrides.quantity ?? 2;
  const unitPrice = overrides.unitPrice ?? 48;

  return {
    id: 'ci1',
    cartId: 'c1',
    productVariantId: 'v1',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    ...overrides,
    quantity,
    unitPrice,
    subtotal: quantity * unitPrice,
  };
}

function cart(items: readonly CartItem[]): CartModel {
  return {
    id: 'c1',
    userId: 'u1',
    items,
    itemCount: items.length,
    totalAmount: items.reduce((total, line) => total + line.subtotal, 0),
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    createdById: 'u1',
    updatedById: null,
  };
}

function variant(overrides: Partial<ProductVariant> = {}): ProductVariant {
  return {
    id: 'v1',
    productId: 'p1',
    productName: 'Radiance Renewal Serum',
    sku: 'GLOW-SER-30',
    ...overrides,
  };
}

function page<T>(data: readonly T[], totalCount = data.length): PaginatedResult<T> {
  return { totalCount, data };
}

describe('Cart', () => {
  let fixture: ComponentFixture<Cart>;
  let element: HTMLElement;
  let http: HttpTestingController;
  let toasts: ToastService;

  const lineNames = () =>
    Array.from(element.querySelectorAll('.line__name')).map((node) => node.textContent?.trim());

  const buttonLabelled = (label: string) =>
    Array.from(element.querySelectorAll<HTMLButtonElement>('button')).find(
      (button) => button.textContent?.trim() === label,
    );

  /**
   * The confirmations repeat the wording of the control that opened them - "Remove",
   * "Empty the cart" - so the dialog's own button has to be picked out by name.
   */
  const confirmButtonLabelled = (label: string) =>
    Array.from(element.querySelectorAll<HTMLButtonElement>('.confirm__actions button')).find(
      (button) => button.textContent?.trim() === label,
    );

  const render = async () => {
    await Promise.resolve();
    fixture.detectChanges();
  };

  /**
   * The cart is fetched from an effect, so change detection has to run - but never
   * await stability first: an open request keeps the app unstable.
   */
  const pendingRequests = (url: string, method?: string) => {
    fixture.detectChanges();
    return http.match((request) => request.url === url && (!method || request.method === method));
  };

  /** Answers the lookup that labels the lines. */
  const flushVariants = async (variants: readonly ProductVariant[] = [variant()]) => {
    pendingRequests('/api/productvariants').forEach((request) => request.flush(page(variants)));
    await render();
  };

  /** Answers the pending cart read and re-renders with the result. */
  const flushCart = async (
    result: CartModel,
    variants: readonly ProductVariant[] = [variant()],
  ) => {
    const latest = pendingRequests('/api/carts/me', 'GET').at(-1);
    latest?.flush(result);
    await render();
    await flushVariants(variants);
    return latest;
  };

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [Cart],
      providers: [
        provideRouter([]),
        // The page reports failures through the interceptor, so it is part of
        // what these tests exercise.
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    toasts = TestBed.inject(ToastService);
    // A cart belongs to an account, so every one of these starts signed in.
    TestBed.inject(AuthStore).set({ accessToken: tokenFor('a@b.c'), refreshToken: 'r1' });

    fixture = TestBed.createComponent(Cart);
    element = fixture.nativeElement as HTMLElement;
  });

  afterEach(() => {
    http.verify({ ignoreCancelled: true });
    toasts.clear();
    localStorage.clear();
  });

  it('lists the lines the API returns, labelled by their variant', async () => {
    await flushCart(
      cart([item(), item({ id: 'ci2', productVariantId: 'v2', quantity: 1, unitPrice: 32 })]),
      [variant(), variant({ id: 'v2', productName: 'Gentle Milk Cleanser', sku: 'MILK-CLN-150' })],
    );

    expect(lineNames()).toEqual(['Radiance Renewal Serum', 'Gentle Milk Cleanser']);
    expect(element.querySelectorAll('.line__sku')[0].textContent?.trim()).toBe('GLOW-SER-30');
  });

  it('counts units rather than lines, and shows the total the API worked out', async () => {
    await flushCart(cart([item({ quantity: 2 }), item({ id: 'ci2', quantity: 3, unitPrice: 10 })]));

    const lede = element.querySelector('.cart__lede')?.textContent;
    expect(lede).toContain('5 items');
    expect(lede).toContain('2 lines');
    expect(element.querySelector('.summary__row--total')?.textContent).toContain('126');
  });

  it('sends the new quantity outright rather than a delta', async () => {
    await flushCart(cart([item({ quantity: 2 })]));

    const steppers = element.querySelectorAll<HTMLButtonElement>('.stepper__btn');
    steppers[steppers.length - 1].click();

    const request = pendingRequests('/api/carts/me/items/ci1', 'PATCH').at(-1);
    expect(request?.request.body).toEqual({ quantity: 3 });
    request?.flush(cart([item({ quantity: 3 })]));
    await render();

    expect(element.querySelector('.cart__lede')?.textContent).toContain('3 items');
  });

  it('asks to confirm instead of sending a quantity of zero, which the API refuses', async () => {
    await flushCart(cart([item({ quantity: 1 })]));

    element.querySelector<HTMLButtonElement>('.stepper__btn')!.click();
    await render();

    // No request went out; the shopper is asked whether they meant to remove it.
    expect(http.match('/api/carts/me/items/ci1').length).toBe(0);
    expect(element.querySelector('.confirm__title')?.textContent).toContain('Remove');
  });

  it('removes a line once the removal is confirmed', async () => {
    await flushCart(cart([item()]));

    buttonLabelled('Remove')!.click();
    await render();
    confirmButtonLabelled('Remove')!.click();

    const request = pendingRequests('/api/carts/me/items/ci1', 'DELETE').at(-1);
    expect(request).toBeDefined();
    request?.flush(cart([]));
    await render();

    expect(element.querySelector('.empty__title')?.textContent).toContain('Your cart is empty');
  });

  it('leaves the line alone when the removal is cancelled', async () => {
    await flushCart(cart([item()]));

    buttonLabelled('Remove')!.click();
    await render();
    confirmButtonLabelled('Keep it')!.click();
    await render();

    expect(http.match('/api/carts/me/items/ci1').length).toBe(0);
    expect(lineNames()).toEqual(['Radiance Renewal Serum']);
  });

  it('empties every line with one request once that is confirmed', async () => {
    await flushCart(cart([item(), item({ id: 'ci2' })]));

    buttonLabelled('Empty the cart')!.click();
    await render();
    confirmButtonLabelled('Empty the cart')!.click();

    const request = pendingRequests('/api/carts/me/items', 'DELETE').at(-1);
    expect(request).toBeDefined();
    request?.flush(cart([]));
    await render();

    expect(element.querySelector('.empty__title')?.textContent).toContain('Your cart is empty');
  });

  it('keeps a line whose variant the lookup cannot name, price and all', async () => {
    await flushCart(cart([item({ productVariantId: 'gone' })]), []);

    expect(lineNames()).toEqual(['This item']);
    expect(element.querySelector('.line__sku--missing')).not.toBeNull();
    expect(element.querySelector('.line__subtotal')?.textContent).toContain('96');
  });

  it('reports a failed read where the lines would be, with a way to retry', async () => {
    pendingRequests('/api/carts/me', 'GET')
      .at(-1)
      ?.flush('Nope', { status: 500, statusText: 'Server Error' });
    await render();
    await flushVariants();

    expect(element.querySelector('.notice__title')?.textContent).toBeTruthy();

    buttonLabelled('Try again')!.click();
    const retried = pendingRequests('/api/carts/me', 'GET').at(-1);
    expect(retried).toBeDefined();
    retried?.flush(cart([item()]));
    await render();
    await flushVariants();

    expect(lineNames()).toEqual(['Radiance Renewal Serum']);
  });
});
