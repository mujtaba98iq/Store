import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import { Toast, ToastKind, ToastOptions } from '@app/core/models/interfaces/toast';

/**
 * How long each kind stays up. A failure takes longer to read than a
 * confirmation, and the reader may still have to act on it.
 */
const DEFAULT_DURATION: Readonly<Record<ToastKind, number>> = {
  [ToastKind.Success]: 4_000,
  [ToastKind.Info]: 5_000,
  [ToastKind.Warning]: 6_500,
  [ToastKind.Error]: 8_000,
};

/** Beyond this the stack starts covering the page, so the oldest makes room. */
const MAX_VISIBLE = 4;

/** Matches the exit animation in the host's stylesheet. */
const LEAVE_MS = 220;

interface Countdown {
  /** `null` while the countdown is paused, or while nothing is scheduled. */
  handle: ReturnType<typeof setTimeout> | null;
  /** Milliseconds left when the countdown was last paused. */
  remaining: number;
  startedAt: number;
}

/**
 * The app's notification channel. Anything worth telling the reader about goes
 * through here: the CRUD stores announce what succeeded and `errorInterceptor`
 * announces what the API refused, so no component owns error reporting of its own.
 *
 * Root-scoped, and rendered once by `app-toast-host`.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly items = signal<readonly Toast[]>([]);
  private readonly countdowns = new Map<number, Countdown>();

  private nextId = 1;
  private isPaused = false;

  /** Newest first, so an arriving toast never shifts the ones being read. */
  readonly toasts = this.items.asReadonly();

  constructor() {
    inject(DestroyRef).onDestroy(() => this.clear());
  }

  success(message: string, options?: ToastOptions): number {
    return this.show(ToastKind.Success, message, options);
  }

  error(message: string, options?: ToastOptions): number {
    return this.show(ToastKind.Error, message, options);
  }

  warning(message: string, options?: ToastOptions): number {
    return this.show(ToastKind.Warning, message, options);
  }

  info(message: string, options?: ToastOptions): number {
    return this.show(ToastKind.Info, message, options);
  }

  /**
   * Queues a notification and returns its id. The same message arriving while it
   * is still up counts up on the toast already there instead of stacking a second
   * copy - a burst of failures reads as one problem rather than four.
   */
  show(kind: ToastKind, message: string, options: ToastOptions = {}): number {
    const text = message.trim();
    if (!text) {
      return 0;
    }

    const title = options.title?.trim() || null;
    const duration = options.duration ?? DEFAULT_DURATION[kind];

    const duplicate = this.items().find(
      (toast) =>
        !toast.leaving && toast.kind === kind && toast.message === text && toast.title === title,
    );

    if (duplicate) {
      this.items.update((toasts) =>
        toasts.map((toast) =>
          toast.id === duplicate.id ? { ...toast, repeats: toast.repeats + 1 } : toast,
        ),
      );
      this.startCountdown(duplicate.id, duration);
      return duplicate.id;
    }

    const toast: Toast = {
      id: this.nextId++,
      kind,
      message: text,
      title,
      repeats: 1,
      leaving: false,
    };

    this.items.update((toasts) => [toast, ...toasts]);
    this.trim();
    this.startCountdown(toast.id, duration);
    return toast.id;
  }

  /** Plays the exit animation, then drops the toast. */
  dismiss(id: number): void {
    const toast = this.items().find((candidate) => candidate.id === id);
    if (!toast || toast.leaving) {
      return;
    }

    this.stopCountdown(id);
    this.items.update((toasts) =>
      toasts.map((candidate) =>
        candidate.id === id ? { ...candidate, leaving: true } : candidate,
      ),
    );

    const handle = setTimeout(() => {
      this.countdowns.delete(id);
      this.items.update((toasts) => toasts.filter((candidate) => candidate.id !== id));
    }, LEAVE_MS);

    this.countdowns.set(id, { handle, remaining: LEAVE_MS, startedAt: Date.now() });
  }

  /** Drops everything at once. */
  clear(): void {
    for (const countdown of this.countdowns.values()) {
      this.cancel(countdown);
    }
    this.countdowns.clear();
    this.items.set([]);
  }

  /**
   * Holds every countdown while the reader is hovering over or tabbing through the
   * stack, so a toast cannot vanish out from under the pointer or the keyboard.
   */
  pause(): void {
    if (this.isPaused) {
      return;
    }
    this.isPaused = true;

    const now = Date.now();
    for (const countdown of this.countdowns.values()) {
      if (countdown.handle === null) {
        continue;
      }
      this.cancel(countdown);
      countdown.remaining = Math.max(0, countdown.remaining - (now - countdown.startedAt));
    }
  }

  resume(): void {
    if (!this.isPaused) {
      return;
    }
    this.isPaused = false;

    for (const [id, countdown] of this.countdowns) {
      this.schedule(id, countdown, countdown.remaining);
    }
  }

  private startCountdown(id: number, duration: number): void {
    this.stopCountdown(id);
    if (duration <= 0) {
      return;
    }

    const countdown: Countdown = { handle: null, remaining: duration, startedAt: Date.now() };
    this.countdowns.set(id, countdown);

    if (!this.isPaused) {
      this.schedule(id, countdown, duration);
    }
  }

  private schedule(id: number, countdown: Countdown, delay: number): void {
    countdown.startedAt = Date.now();
    countdown.remaining = delay;
    countdown.handle = setTimeout(() => this.dismiss(id), delay);
  }

  private stopCountdown(id: number): void {
    const countdown = this.countdowns.get(id);
    if (countdown) {
      this.cancel(countdown);
      this.countdowns.delete(id);
    }
  }

  private cancel(countdown: Countdown): void {
    if (countdown.handle !== null) {
      clearTimeout(countdown.handle);
      countdown.handle = null;
    }
  }

  /** Sends the oldest toasts out once the stack is taller than the page allows. */
  private trim(): void {
    const alive = this.items().filter((toast) => !toast.leaving);
    for (const toast of alive.slice(MAX_VISIBLE)) {
      this.dismiss(toast.id);
    }
  }
}
