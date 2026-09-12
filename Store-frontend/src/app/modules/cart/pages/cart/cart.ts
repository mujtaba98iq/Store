import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Modal } from '@app/shared/components/modal/modal';
import { CartLine, CartPageStore } from '../../data-access/cart-page-store';

/**
 * The shopper's cart: what is in it, how many of each, and what it comes to.
 *
 * Behind `authGuard` - the API addresses a cart by the caller's token, so there is
 * nothing here to show a visitor without one.
 */
@Component({
  selector: 'app-cart',
  imports: [CurrencyPipe, RouterLink, Modal],
  templateUrl: './cart.html',
  styleUrl: './cart.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [CartPageStore],
})
export class Cart {
  private readonly store = inject(CartPageStore);

  protected readonly lines = this.store.lines;
  protected readonly isLoading = this.store.isLoading;
  protected readonly isLoaded = this.store.isLoaded;
  protected readonly isEmpty = this.store.isEmpty;
  protected readonly errorMessage = this.store.errorMessage;
  protected readonly totalAmount = this.store.totalAmount;
  protected readonly unitCount = this.store.unitCount;
  protected readonly lineCount = this.store.lineCount;
  protected readonly pendingItemId = this.store.pendingItemId;
  protected readonly isClearing = this.store.isClearing;
  protected readonly isBusy = this.store.isBusy;

  protected readonly pendingRemoval = this.store.pendingRemoval;
  protected readonly isClearConfirmOpen = this.store.isClearConfirmOpen;

  protected stepQuantity(line: CartLine, by: number): void {
    this.store.setQuantity(line, line.item.quantity + by);
  }

  /**
   * The box keeps whatever was typed even when the cart does not take it - a zero
   * that opens the removal dialog and is then cancelled, a value the API refuses -
   * and the binding would not redraw it, because the line never changed. So it is
   * put back to what the line actually holds, and the binding overwrites that
   * again if the change goes through.
   */
  protected onQuantityInput(line: CartLine, event: Event): void {
    const input = event.target as HTMLInputElement;
    const typed = Number(input.value);

    input.value = String(line.item.quantity);
    this.store.setQuantity(line, Number.isFinite(typed) ? Math.trunc(typed) : line.item.quantity);
  }

  protected askToRemove(line: CartLine): void {
    this.store.askToRemove(line);
  }

  protected cancelRemoval(): void {
    this.store.cancelRemoval();
  }

  protected confirmRemoval(): void {
    this.store.confirmRemoval();
  }

  protected askToClear(): void {
    this.store.askToClear();
  }

  protected cancelClear(): void {
    this.store.cancelClear();
  }

  protected confirmClear(): void {
    this.store.confirmClear();
  }

  protected retry(): void {
    this.store.reload();
  }
}
