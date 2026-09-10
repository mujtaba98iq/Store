import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

/** Centred overlay panel. Closes on backdrop click or Escape. */
@Component({
  selector: 'app-modal',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '(document:keydown.escape)': 'closed.emit()' },
  template: `
    <button
      class="modal__backdrop"
      type="button"
      [attr.aria-label]="'Close ' + label()"
      (click)="closed.emit()"
    ></button>
    <div class="modal__panel" role="dialog" aria-modal="true" [attr.aria-label]="label()">
      <ng-content />
    </div>
  `,
  styles: `
    :host {
      position: fixed;
      inset: 0;
      z-index: 20;
      display: grid;
      place-items: center;
      padding: clamp(1rem, 4vw, 2.5rem);
    }

    .modal__backdrop {
      position: absolute;
      inset: 0;
      padding: 0;
      background: rgba(36, 31, 27, 0.45);
      border: 0;
      cursor: pointer;
    }

    .modal__panel {
      position: relative;
      width: min(38rem, 100%);
      max-height: 100%;
      overflow-y: auto;
      padding: clamp(1.25rem, 3vw, 2rem);
      background: var(--STOR-surface);
      border-radius: var(--radius-card);
      box-shadow: 0 30px 80px rgba(36, 31, 27, 0.32);
    }
  `,
})
export class Modal {
  readonly label = input('dialog');
  readonly closed = output<void>();
}
