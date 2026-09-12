import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

/** Stands in for a run of page numbers the pager left out. */
const GAP = 0;

/**
 * The numbered pager the management listings share. It prints the first and last
 * page always, the current page with a neighbour either side, and an ellipsis
 * wherever a run was skipped - so a long listing keeps a pager of a fixed width.
 */
@Component({
  selector: 'app-pager',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <nav class="pager" [attr.aria-label]="label()">
      <button
        class="pager__step"
        type="button"
        aria-label="Previous page"
        [disabled]="page() === 1"
        (click)="pageChange.emit(page() - 1)"
      >
        ‹
      </button>
      @for (slot of slots(); track $index) {
        @if (slot === GAP) {
          <span class="pager__gap" aria-hidden="true">…</span>
        } @else {
          <button
            class="pager__page"
            type="button"
            [class.pager__page--current]="slot === page()"
            [attr.aria-current]="slot === page() ? 'page' : null"
            [attr.aria-label]="'Page ' + slot"
            (click)="pageChange.emit(slot)"
          >
            {{ slot }}
          </button>
        }
      }
      <button
        class="pager__step"
        type="button"
        aria-label="Next page"
        [disabled]="page() === totalPages()"
        (click)="pageChange.emit(page() + 1)"
      >
        ›
      </button>
    </nav>
  `,
  styles: `
    :host {
      display: block;
      margin-top: clamp(1rem, 2vw, 1.5rem);
    }

    .pager {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      justify-content: flex-end;
      gap: 0.25rem;
    }

    .pager__page,
    .pager__step {
      min-width: 2.25rem;
      height: 2.25rem;
      padding: 0 0.5rem;
      background: var(--STOR-surface);
      border: 1px solid var(--STOR-line);
      border-radius: 2px;
      color: var(--STOR-ink-soft);
      font-size: 0.875rem;
      font-variant-numeric: tabular-nums;
      cursor: pointer;
      transition:
        background-color 0.2s ease,
        border-color 0.2s ease,
        color 0.2s ease;
    }

    .pager__page:hover:not(:disabled),
    .pager__step:hover:not(:disabled) {
      background: #f6f1ea;
      color: var(--STOR-ink);
    }

    .pager__page--current,
    .pager__page--current:hover {
      background: var(--STOR-ink);
      border-color: var(--STOR-ink);
      color: #fff;
    }

    .pager__step:disabled {
      opacity: 0.4;
      cursor: not-allowed;
    }

    .pager__gap {
      padding-inline: 0.25rem;
      color: var(--STOR-ink-soft);
    }
  `,
})
export class Pager {
  readonly page = input.required<number>();
  readonly totalPages = input.required<number>();

  /** Names the listing being paged, for the reader who cannot see the rows. */
  readonly label = input('Pages');

  readonly pageChange = output<number>();

  protected readonly GAP = GAP;

  protected readonly slots = computed<readonly number[]>(() => {
    const total = this.totalPages();
    const current = this.page();
    const shown = new Set([1, total, current - 1, current, current + 1]);
    const slots: number[] = [];

    for (let page = 1; page <= total; page += 1) {
      if (shown.has(page)) {
        slots.push(page);
      } else if (slots.at(-1) !== GAP) {
        slots.push(GAP);
      }
    }

    return slots;
  });
}
