/** The four shapes a notification can take, in rising order of urgency. */
export const ToastKind = {
  Success: 'success',
  Info: 'info',
  Warning: 'warning',
  Error: 'error',
} as const;
export type ToastKind = (typeof ToastKind)[keyof typeof ToastKind];

/** One notification on screen. Owned by `ToastService`; the host only renders it. */
export interface Toast {
  readonly id: number;
  readonly kind: ToastKind;
  /** The sentence the reader sees. Already user-facing - never a raw exception. */
  readonly message: string;
  /** Optional heading above the message. */
  readonly title: string | null;
  /** How many times the same notification arrived while this one was up. */
  readonly repeats: number;
  /** Set while the exit animation plays, just before the toast is dropped. */
  readonly leaving: boolean;
}

export interface ToastOptions {
  readonly title?: string;
  /** Milliseconds on screen. `0` keeps it up until it is dismissed. */
  readonly duration?: number;
}
