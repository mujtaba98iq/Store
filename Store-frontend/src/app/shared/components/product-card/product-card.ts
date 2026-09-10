import { CurrencyPipe, NgOptimizedImage } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  linkedSignal,
  output,
} from '@angular/core';

@Component({
  selector: 'app-product-card',
  imports: [CurrencyPipe, NgOptimizedImage],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <article class="card">
      <div class="card__body">
        <div class="card__head">
          <p class="card__category">{{ category() }}</p>
          @if (editable()) {
            <button
              class="card__edit"
              type="button"
              (click)="edit.emit()"
              [attr.aria-label]="'Edit ' + name()"
            >
              <svg
                viewBox="0 0 24 24"
                width="18"
                height="18"
                fill="none"
                stroke="currentColor"
                stroke-width="1.7"
                stroke-linecap="round"
                stroke-linejoin="round"
                aria-hidden="true"
              >
                <path d="M20 13.4V19a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h5.6" />
                <path d="M18.4 3.6a1.9 1.9 0 0 1 2.7 2.7l-8.4 8.4-3.4.7.7-3.4z" />
              </svg>
            </button>
          }
        </div>
        <h2 class="card__name">{{ name() }}</h2>
        <p class="card__price">{{ price() | currency: currencyCode() }}</p>
        <div class="card__media">
          @if (showPlaceholder()) {
            <div class="card__fallback" aria-hidden="true">
              <svg viewBox="0 0 48 48" width="32" height="32" fill="none">
                <rect
                  x="15"
                  y="18"
                  width="18"
                  height="22"
                  rx="4"
                  stroke="currentColor"
                  stroke-width="1.6"
                />
                <path
                  d="M20 18v-6h8v6"
                  stroke="currentColor"
                  stroke-width="1.6"
                  stroke-linejoin="round"
                />
              </svg>
            </div>
          } @else {
            <img
              [ngSrc]="image()"
              width="320"
              height="320"
              [alt]="name()"
              (error)="imageFailed.set(true)"
            />
          }
        </div>
        @if (description().trim()) {
          <p class="card__description">{{ description() }}</p>
        }
      </div>
      <button
        class="card__add"
        type="button"
        (click)="add.emit()"
        [attr.aria-label]="'Add ' + name() + ' to cart'"
      >
        <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
          <path
            d="M12 5v14M5 12h14"
            stroke="currentColor"
            stroke-width="1.9"
            stroke-linecap="round"
          />
        </svg>
        <span>Add to Cart</span>
      </button>
    </article>
  `,
  styles: `
    :host {
      display: block;
      /* Type and spacing scale with the card, so the same component reads well
         at hero size and at catalogue size. */
      container-type: inline-size;
    }

    .card {
      /* The host sizes the card; --card-media-height tunes the image band. */
      --card-pad: clamp(0.7rem, 4.5cqi, 1.05rem);
      --card-gap: clamp(0.5rem, 3cqi, 0.9rem);

      display: flex;
      flex-direction: column;
      height: 100%;
      padding: var(--card-pad);
      background: var(--STOR-surface);
      border-radius: var(--radius-card);
      box-shadow: 0 18px 44px rgba(72, 50, 26, 0.16);
    }

    .card__body {
      /* Grows so the button stays pinned to the bottom edge across a row. */
      flex: 1 1 auto;
    }

    .card__head {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 0.5rem;
    }

    .card__category {
      /* Holds its line even when a product has no category, so the cards in a
         row stay the same height. 1.5em matches the inherited line-height. */
      min-height: 1.5em;
      font-size: clamp(0.625rem, 3.2cqi, 0.75rem);
      font-weight: 500;
      letter-spacing: 0.14em;
      text-transform: uppercase;
      color: var(--STOR-ink-soft);
      opacity: 0.75;
    }

    .card__name {
      /* Two lines held open, like the category line, so the image band starts
         at the same height across a row. */
      min-height: 2.3em;
      margin-top: 0.35rem;
      font-family: var(--font-display);
      font-size: clamp(0.9375rem, 8cqi, 1.75rem);
      font-weight: 400;
      line-height: 1.15;
      letter-spacing: -0.01em;
      color: var(--STOR-ink);
    }

    .card__price {
      margin-top: 0.15rem;
      font-family: var(--font-display);
      font-size: clamp(0.9375rem, 8cqi, 1.75rem);
      font-weight: 400;
      line-height: 1.15;
    }

    .card__media {
      margin-top: var(--card-gap);
      overflow: hidden;
      border-radius: calc(var(--radius-card) - 5px);
    }

    .card__fallback {
      display: grid;
      place-items: center;
      aspect-ratio: 1;
      max-height: var(--card-media-height, none);
      background: var(--STOR-cream);
      color: var(--STOR-ink-soft);
      opacity: 0.7;
    }

    .card__media img {
      width: 100%;
      height: auto;
      aspect-ratio: 1;
      max-height: var(--card-media-height, none);
      object-fit: cover;
    }

    .card__description {
      /* Clamped so a long copy block never stretches one card past its row. */
      display: -webkit-box;
      -webkit-box-orient: vertical;
      -webkit-line-clamp: 3;
      line-clamp: 3;
      overflow: hidden;
      margin-top: var(--card-gap);
      color: var(--STOR-ink-soft);
      font-size: clamp(0.75rem, 4.2cqi, 0.9375rem);
      line-height: 1.45;
    }

    .card__add {
      display: flex;
      flex: none;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      width: 100%;
      min-height: clamp(2.25rem, 13cqi, 3.1rem);
      margin-top: var(--card-gap);
      padding: 0 0.75rem;
      background: var(--STOR-accent);
      border: 0;
      border-radius: calc(var(--radius-card) - 4px);
      color: #fff;
      font-size: clamp(0.75rem, 4.4cqi, 1rem);
      font-weight: 500;
      cursor: pointer;
      transition: background-color 0.2s ease;
    }

    .card__add:hover {
      background: var(--STOR-accent-deep);
    }

    .card__edit {
      display: grid;
      flex: none;
      place-items: center;
      padding: 0;
      background: none;
      border: 0;
      color: var(--STOR-ink);
      cursor: pointer;
      transition: opacity 0.2s ease;
    }

    .card__edit:hover {
      opacity: 0.6;
    }
  `,
})
export class ProductCard {
  readonly name = input.required<string>();
  readonly price = input.required<number>();
  readonly image = input.required<string>();
  readonly category = input('');
  readonly description = input('');
  readonly editable = input(false);
  readonly currencyCode = input('USD');

  readonly add = output<void>();
  readonly edit = output<void>();

  /** Stored image paths can be stale; fall back rather than collapse the card. */
  protected readonly imageFailed = linkedSignal<string, boolean>({
    source: this.image,
    computation: () => false,
  });

  // NgOptimizedImage throws on an empty ngSrc, so an absent image never reaches it.
  protected readonly showPlaceholder = computed(() => !this.image().trim() || this.imageFailed());
}
