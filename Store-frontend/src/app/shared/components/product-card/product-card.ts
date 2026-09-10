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
      @if (editable()) {
        <button
          class="card__edit"
          type="button"
          (click)="edit.emit()"
          [attr.aria-label]="'Edit ' + name()"
        >
          <svg viewBox="0 0 24 24" width="15" height="15" fill="none" aria-hidden="true">
            <path
              d="M4 20h4L19 9l-4-4L4 16z"
              stroke="currentColor"
              stroke-width="1.6"
              stroke-linejoin="round"
            />
          </svg>
        </button>
      }
      @if (category()) {
        <p class="card__category">{{ category() }}</p>
      }
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
      <button
        class="card__add"
        type="button"
        (click)="add.emit()"
        [attr.aria-label]="'Add ' + name() + ' to bag'"
      >
        <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
          <path
            d="M12 5v14M5 12h14"
            stroke="currentColor"
            stroke-width="1.8"
            stroke-linecap="round"
          />
        </svg>
      </button>
    </article>
  `,
  styles: `
    :host {
      display: block;
    }

    .card {
      position: relative;
      /* The host sizes the card; --card-media-height tunes the image band. */
      width: 100%;
      padding: 0.9rem 0.9rem 0;
      background: var(--STOR-surface);
      border-radius: var(--radius-card);
      box-shadow: 0 18px 44px rgba(72, 50, 26, 0.16);
    }

    .card__category {
      font-size: 0.6875rem;
      font-weight: 500;
      letter-spacing: 0.14em;
      text-transform: uppercase;
      color: var(--STOR-ink-soft);
      opacity: 0.75;
    }

    .card__category + .card__name {
      margin-top: 0.3rem;
    }

    .card__name {
      font-family: var(--font-sans);
      font-size: 0.8125rem;
      font-weight: 500;
      line-height: 1.3;
      color: var(--STOR-ink-soft);
    }

    .card__price {
      margin-top: 0.35rem;
      font-size: 1.0625rem;
      font-weight: 600;
    }

    .card__media {
      margin-top: 0.7rem;
      overflow: hidden;
      border-radius: 0 0 var(--radius-card) var(--radius-card);
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

    .card__add {
      position: absolute;
      bottom: -1.1rem;
      left: 50%;
      display: grid;
      place-items: center;
      width: 2.25rem;
      height: 2.25rem;
      padding: 0;
      background: var(--STOR-surface);
      border: 1px solid var(--STOR-line);
      border-radius: 50%;
      box-shadow: 0 6px 16px rgba(72, 50, 26, 0.18);
      cursor: pointer;
      transform: translateX(-50%);
      transition:
        background-color 0.2s ease,
        color 0.2s ease;
    }

    .card__add:hover {
      background: var(--STOR-ink);
      color: #fff;
    }

    .card__edit {
      position: absolute;
      top: 0.55rem;
      inset-inline-end: 0.55rem;
      display: grid;
      place-items: center;
      width: 1.85rem;
      height: 1.85rem;
      padding: 0;
      background: rgba(255, 255, 255, 0.9);
      border: 1px solid var(--STOR-line);
      border-radius: 50%;
      color: var(--STOR-ink-soft);
      cursor: pointer;
      transition:
        background-color 0.2s ease,
        color 0.2s ease;
    }

    .card__edit:hover {
      background: var(--STOR-ink);
      color: #fff;
    }
  `,
})
export class ProductCard {
  readonly name = input.required<string>();
  readonly price = input.required<number>();
  readonly image = input.required<string>();
  readonly category = input('');
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
