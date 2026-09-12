import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Toast, ToastKind } from '@app/core/models/interfaces/toast';
import { ToastService } from '@app/core/services/common/toast';

/**
 * The notification stack, mounted once by the shell. Anchored top-left, and
 * click-through everywhere except on a toast, so the header underneath stays
 * usable; on a narrow screen it spans the width instead.
 *
 * Nothing pushes toasts in here - the stack only renders what `ToastService` holds.
 */
@Component({
  selector: 'app-toast-host',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'region',
    'aria-label': 'Notifications',
  },
  template: `
    <ol class="stack" aria-live="polite" aria-relevant="additions text">
      @for (toast of toasts(); track toast.id) {
        <!--
          The countdown is held on the toast itself rather than on the host: the
          host is click-through so the page underneath stays usable, which leaves
          the toasts the only elements the pointer can actually land on.
        -->
        <li
          class="toast"
          [class]="modifiersFor(toast)"
          [attr.role]="roleFor(toast)"
          (keydown.escape)="dismiss(toast.id)"
          (pointerenter)="pause()"
          (pointerleave)="resume()"
          (focusin)="pause()"
          (focusout)="resume()"
        >
          <span class="toast__icon" aria-hidden="true">
            @switch (toast.kind) {
              @case ('success') {
                <svg viewBox="0 0 20 20" width="18" height="18">
                  <path
                    d="m5 10.4 3.3 3.3L15 7"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                  />
                </svg>
              }
              @case ('error') {
                <svg viewBox="0 0 20 20" width="18" height="18">
                  <path
                    d="m6 6 8 8M14 6l-8 8"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                  />
                </svg>
              }
              @case ('warning') {
                <svg viewBox="0 0 20 20" width="18" height="18">
                  <path
                    d="M10 3.5 18 16.5H2z"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.7"
                    stroke-linejoin="round"
                  />
                  <path
                    d="M10 8.2v3.2"
                    stroke="currentColor"
                    stroke-width="1.7"
                    stroke-linecap="round"
                  />
                  <circle cx="10" cy="13.9" r="0.95" fill="currentColor" />
                </svg>
              }
              @default {
                <svg viewBox="0 0 20 20" width="18" height="18">
                  <circle
                    cx="10"
                    cy="10"
                    r="7.4"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.7"
                  />
                  <path
                    d="M10 9.2v4.6"
                    stroke="currentColor"
                    stroke-width="1.7"
                    stroke-linecap="round"
                  />
                  <circle cx="10" cy="6.3" r="0.95" fill="currentColor" />
                </svg>
              }
            }
          </span>

          <div class="toast__body">
            @if (toast.title) {
              <p class="toast__title">{{ toast.title }}</p>
            }
            <p class="toast__message">{{ toast.message }}</p>
          </div>

          @if (toast.repeats > 1) {
            <span class="toast__repeats" [attr.aria-label]="repeatsLabel(toast)">
              &times;{{ toast.repeats }}
            </span>
          }

          <button
            class="toast__close"
            type="button"
            aria-label="Dismiss notification"
            (click)="dismiss(toast.id)"
          >
            <svg viewBox="0 0 16 16" width="14" height="14" aria-hidden="true">
              <path
                d="m4 4 8 8M12 4l-8 8"
                fill="none"
                stroke="currentColor"
                stroke-width="1.6"
                stroke-linecap="round"
              />
            </svg>
          </button>
        </li>
      }
    </ol>
  `,
  styles: `
    :host {
      /* The site header scrolls with the page rather than sticking to it, so the
         stack starts below it: its own vertical padding, plus the 2.5rem icon
         buttons that set the row's height. Keep in step with .site-header in
         core/layout/header/header.scss. Without this the first toast covers the
         brand and the Products link for as long as it is up. */
      --header-height: calc(clamp(0.9rem, 1.8vw, 1.4rem) * 2 + 2.5rem);
      --gutter: clamp(0.75rem, 2vw, 1.25rem);

      position: fixed;
      left: 0;
      top: 0;
      z-index: 60;
      display: block;
      width: min(26rem, 100%);
      padding: calc(var(--header-height) + var(--gutter)) var(--gutter) var(--gutter);
      /* See-through to the page except where a toast actually is. */
      pointer-events: none;
    }

    .stack {
      display: flex;
      /* The list is newest-first, which already puts the newest at the top edge. */
      flex-direction: column;
      gap: 0.625rem;
      margin: 0;
      padding: 0;
      list-style: none;
    }

    .toast {
      --toast-accent: var(--STOR-accent);

      display: grid;
      grid-template-columns: auto minmax(0, 1fr) auto auto;
      align-items: start;
      gap: 0.6875rem;
      pointer-events: auto;
      padding: 0.8125rem 0.875rem;
      background: var(--STOR-surface);
      color: var(--STOR-ink);
      border: 1px solid var(--STOR-line);
      border-left: 3px solid var(--toast-accent);
      border-radius: var(--radius-card);
      box-shadow: 0 14px 34px rgba(36, 31, 27, 0.18);
      animation: toast-in 0.24s cubic-bezier(0.22, 1, 0.36, 1);
    }

    .toast--success {
      --toast-accent: #1f6b45;
    }

    .toast--info {
      --toast-accent: var(--STOR-accent);
    }

    .toast--warning {
      --toast-accent: #8a5a0f;
    }

    .toast--error {
      --toast-accent: #7c2016;
    }

    .toast--leaving {
      animation: toast-out 0.22s ease forwards;
    }

    .toast__icon {
      display: grid;
      place-items: center;
      width: 1.5rem;
      height: 1.5rem;
      border-radius: 50%;
      background: color-mix(in srgb, var(--toast-accent) 12%, transparent);
      color: var(--toast-accent);
    }

    .toast__body {
      min-width: 0;
      padding-top: 0.0625rem;
    }

    .toast__title {
      font-size: 0.875rem;
      font-weight: 600;
      letter-spacing: 0.01em;
    }

    .toast__message {
      font-size: 0.875rem;
      line-height: 1.45;
      color: var(--STOR-ink-soft);
      /* API messages can be long and unbroken; wrap rather than widen the panel. */
      overflow-wrap: anywhere;
    }

    .toast__title + .toast__message {
      margin-top: 0.125rem;
    }

    .toast__repeats {
      padding: 0.125rem 0.375rem;
      border-radius: 999px;
      background: color-mix(in srgb, var(--toast-accent) 12%, transparent);
      color: var(--toast-accent);
      font-size: 0.75rem;
      font-weight: 600;
      font-variant-numeric: tabular-nums;
    }

    .toast__close {
      display: grid;
      place-items: center;
      width: 1.5rem;
      height: 1.5rem;
      padding: 0;
      background: none;
      border: 0;
      border-radius: 4px;
      color: var(--STOR-ink-soft);
      opacity: 0.7;
      cursor: pointer;
      transition:
        opacity 0.15s ease,
        background-color 0.15s ease;
    }

    .toast__close:hover {
      opacity: 1;
      background: rgba(36, 31, 27, 0.07);
    }

    /* In and out past the left edge the stack is anchored to. */
    @keyframes toast-in {
      from {
        opacity: 0;
        transform: translate3d(-0.75rem, 0, 0) scale(0.98);
      }
    }

    @keyframes toast-out {
      to {
        opacity: 0;
        transform: translate3d(-0.5rem, 0, 0) scale(0.97);
      }
    }

    @media (max-width: 40rem) {
      :host {
        width: 100%;
      }
    }
  `,
})
export class ToastHost {
  private readonly service = inject(ToastService);

  protected readonly toasts = this.service.toasts;

  /** A failure interrupts; a confirmation waits its turn in the polite queue. */
  protected roleFor(toast: Toast): 'alert' | 'status' {
    return toast.kind === ToastKind.Error || toast.kind === ToastKind.Warning ? 'alert' : 'status';
  }

  protected modifiersFor(toast: Toast): string {
    return toast.leaving ? `toast--${toast.kind} toast--leaving` : `toast--${toast.kind}`;
  }

  protected repeatsLabel(toast: Toast): string {
    return `Repeated ${toast.repeats} times`;
  }

  protected dismiss(id: number): void {
    this.service.dismiss(id);
  }

  protected pause(): void {
    this.service.pause();
  }

  protected resume(): void {
    this.service.resume();
  }
}
