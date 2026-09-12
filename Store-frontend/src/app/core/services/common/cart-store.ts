import { Injectable, computed, effect, inject, signal, untracked } from '@angular/core';
import { EMPTY, Observable, catchError, finalize, tap } from 'rxjs';
import { Cart, CartItem } from '@app/core/models/interfaces/cart';
import { ApiCartsService } from '@app/core/services/api/carts';
import { AuthStore } from '@app/core/services/common/auth-store';
import { ToastService } from '@app/core/services/common/toast';
import { describeError } from '@app/core/utils/api-error';

/**
 * The shopper's cart, held once for the whole app.
 *
 * Root-scoped on purpose: the header badge, the product page's add button and the
 * cart page are three views of one cart, and a page-scoped copy would let them
 * disagree. Every endpoint answers with the whole cart, so each write ends by
 * replacing what is held here rather than by refetching.
 *
 * The API puts the cart behind the `User,Admin` roles, so nothing is fetched while
 * signed out and the held cart is dropped on sign-out - the next account to sign in
 * on this browser must never see the last one's lines.
 */
@Injectable({ providedIn: 'root' })
export class CartStore {
  private readonly api = inject(ApiCartsService);
  private readonly auth = inject(AuthStore);
  private readonly toasts = inject(ToastService);

  private readonly cartState = signal<Cart | null>(null);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);
  private readonly addingState = signal(false);
  private readonly pendingItemState = signal<string | null>(null);
  private readonly clearingState = signal(false);

  readonly cart = this.cartState.asReadonly();
  readonly isLoading = this.loadingState.asReadonly();
  /** What the last read of the cart failed with, for the page to show and retry. */
  readonly errorMessage = this.errorState.asReadonly();
  readonly isAdding = this.addingState.asReadonly();
  /** The line with a write in flight, so its row can show the progress. */
  readonly pendingItemId = this.pendingItemState.asReadonly();
  readonly isClearing = this.clearingState.asReadonly();

  readonly items = computed<readonly CartItem[]>(() => this.cartState()?.items ?? []);
  readonly totalAmount = computed(() => this.cartState()?.totalAmount ?? 0);
  /** Lines in the cart, as the API counts them. */
  readonly lineCount = computed(() => this.cartState()?.itemCount ?? 0);

  /**
   * Units across every line - what the header badge shows. Two bottles of one
   * serum is a bag with two things in it, however many lines hold them.
   */
  readonly unitCount = computed(() =>
    this.items().reduce((total, item) => total + item.quantity, 0),
  );

  /** Nothing has been fetched yet, so "empty" is not yet known to be true. */
  readonly isLoaded = computed(() => this.cartState() !== null);
  readonly isEmpty = computed(() => this.isLoaded() && this.items().length === 0);

  /** One write at a time: the cart that comes back is the whole cart, and two in
   * flight would race to be the version that sticks. */
  readonly isBusy = computed(
    () => this.addingState() || this.clearingState() || this.pendingItemState() !== null,
  );

  constructor() {
    // The session outlives a reload, so this also fetches on start-up for an
    // account that is already signed in.
    effect(() => {
      const signedIn = this.auth.isSignedIn();
      untracked(() => (signedIn ? this.load() : this.reset()));
    });
  }

  /** Fetches the cart, creating an empty one if this is the shopper's first. */
  load(): void {
    if (!this.auth.isSignedIn() || this.loadingState()) {
      return;
    }

    this.loadingState.set(true);
    this.errorState.set(null);

    this.api
      .mine()
      .pipe(finalize(() => this.loadingState.set(false)))
      .subscribe({
        next: (cart) => this.cartState.set(cart),
        error: (error: unknown) => this.errorState.set(describeError(error)),
      });
  }

  /**
   * Tops up the line for this variant, opening one if the cart has none.
   *
   * Emits the updated cart and announces it; a failure emits nothing and leaves the
   * cart as it was - `errorInterceptor` says what the API refused, which is how the
   * shopper learns that a variant has been taken off sale or has no price.
   *
   * `label` is what the shopper just chose, so the confirmation names it rather
   * than saying "item".
   */
  add(productVariantId: string, quantity: number, label?: string): Observable<Cart> {
    if (this.isBusy()) {
      return EMPTY;
    }

    this.addingState.set(true);

    return this.api.addItem({ productVariantId, quantity }).pipe(
      tap((cart) => {
        this.cartState.set(cart);
        this.toasts.success(label ? `${label} added to your cart.` : 'Added to your cart.');
      }),
      catchError(() => EMPTY),
      finalize(() => this.addingState.set(false)),
    );
  }

  /**
   * Sets the line's quantity. The API refuses zero rather than reading it as a
   * removal, so a shopper stepping down from one is sent to `remove` instead.
   */
  updateQuantity(cartItemId: string, quantity: number): void {
    if (this.isBusy()) {
      return;
    }

    if (quantity < 1) {
      this.remove(cartItemId);
      return;
    }

    this.pendingItemState.set(cartItemId);

    this.api
      .updateItem(cartItemId, { quantity })
      .pipe(finalize(() => this.pendingItemState.set(null)))
      .subscribe({
        next: (cart) => this.cartState.set(cart),
        // Reported by `errorInterceptor`; the line stays at the quantity it held.
        error: () => undefined,
      });
  }

  remove(cartItemId: string): void {
    if (this.isBusy()) {
      return;
    }

    this.pendingItemState.set(cartItemId);

    this.api
      .removeItem(cartItemId)
      .pipe(finalize(() => this.pendingItemState.set(null)))
      .subscribe({
        next: (cart) => {
          this.cartState.set(cart);
          this.toasts.success('Removed from your cart.');
        },
        error: () => undefined,
      });
  }

  clear(): void {
    if (this.isBusy() || this.isEmpty()) {
      return;
    }

    this.clearingState.set(true);

    this.api
      .clear()
      .pipe(finalize(() => this.clearingState.set(false)))
      .subscribe({
        next: (cart) => {
          this.cartState.set(cart);
          this.toasts.success('Your cart is now empty.');
        },
        error: () => undefined,
      });
  }

  /** Drops the held cart. Signing out is the only thing that does this. */
  private reset(): void {
    this.cartState.set(null);
    this.errorState.set(null);
  }
}
