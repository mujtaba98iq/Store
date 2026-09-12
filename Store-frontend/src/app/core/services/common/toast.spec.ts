import { TestBed } from '@angular/core/testing';
import { ToastKind } from '@app/core/models/interfaces/toast';
import { ToastService } from './toast';

/** Long enough to outlast any duration the service uses, plus the exit animation. */
const LONGER_THAN_ANY_TOAST = 20_000;

describe('ToastService', () => {
  let toasts: ToastService;

  const messages = () => toasts.toasts().map((toast) => toast.message);
  const kinds = () => toasts.toasts().map((toast) => toast.kind);

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({});
    toasts = TestBed.inject(ToastService);
  });

  afterEach(() => {
    toasts.clear();
    vi.useRealTimers();
  });

  it('files each helper under its own kind', () => {
    toasts.success('Saved.');
    toasts.error('Refused.');
    toasts.warning('Careful.');
    toasts.info('Noted.');

    // Newest first, so the stack can grow without shifting what is being read.
    expect(kinds()).toEqual([
      ToastKind.Info,
      ToastKind.Warning,
      ToastKind.Error,
      ToastKind.Success,
    ]);
    expect(messages()).toEqual(['Noted.', 'Careful.', 'Refused.', 'Saved.']);
  });

  it('takes a title beside the message', () => {
    toasts.success('Product created successfully.', { title: 'Done' });

    expect(toasts.toasts()[0].title).toBe('Done');
  });

  it('ignores a message with nothing in it', () => {
    expect(toasts.info('   ')).toBe(0);
    expect(toasts.toasts()).toEqual([]);
  });

  it('keeps an error up longer than a success', () => {
    toasts.success('Saved.');
    toasts.error('Refused.');

    vi.advanceTimersByTime(4_500);
    expect(messages()).toEqual(['Refused.']);

    vi.advanceTimersByTime(LONGER_THAN_ANY_TOAST);
    expect(toasts.toasts()).toEqual([]);
  });

  it('stays up until it is dismissed when given no duration', () => {
    toasts.error('Refused.', { duration: 0 });

    vi.advanceTimersByTime(LONGER_THAN_ANY_TOAST);
    expect(messages()).toEqual(['Refused.']);
  });

  it('plays the toast out before dropping it', () => {
    const id = toasts.success('Saved.');

    toasts.dismiss(id);
    expect(toasts.toasts()[0].leaving).toBe(true);

    vi.advanceTimersByTime(300);
    expect(toasts.toasts()).toEqual([]);
  });

  it('counts a repeat up rather than stacking a second copy', () => {
    const first = toasts.error('That item no longer exists.');
    const second = toasts.error('That item no longer exists.');

    expect(second).toBe(first);
    expect(messages()).toEqual(['That item no longer exists.']);
    expect(toasts.toasts()[0].repeats).toBe(2);
  });

  it('treats the same words under a different kind as a different notification', () => {
    toasts.error('Careful.');
    toasts.warning('Careful.');

    expect(kinds()).toEqual([ToastKind.Warning, ToastKind.Error]);
  });

  it('gives a repeat the full time again', () => {
    toasts.success('Saved.');
    vi.advanceTimersByTime(3_000);

    toasts.success('Saved.');
    vi.advanceTimersByTime(3_000);
    expect(messages()).toEqual(['Saved.']);

    vi.advanceTimersByTime(LONGER_THAN_ANY_TOAST);
    expect(toasts.toasts()).toEqual([]);
  });

  it('holds the countdown while the reader is on the stack', () => {
    toasts.success('Saved.');

    toasts.pause();
    vi.advanceTimersByTime(LONGER_THAN_ANY_TOAST);
    expect(messages()).toEqual(['Saved.']);

    toasts.resume();
    vi.advanceTimersByTime(4_500);
    expect(toasts.toasts()).toEqual([]);
  });

  it('leaves a toast raised while paused up until the reader leaves', () => {
    toasts.pause();
    toasts.success('Saved.');

    vi.advanceTimersByTime(LONGER_THAN_ANY_TOAST);
    expect(messages()).toEqual(['Saved.']);

    toasts.resume();
    vi.advanceTimersByTime(4_500);
    expect(toasts.toasts()).toEqual([]);
  });

  it('sends the oldest out once the stack would cover the page', () => {
    for (const word of ['one', 'two', 'three', 'four', 'five']) {
      toasts.info(word);
    }

    vi.advanceTimersByTime(300);
    expect(messages()).toEqual(['five', 'four', 'three', 'two']);
  });

  it('drops everything on clear', () => {
    toasts.success('Saved.');
    toasts.error('Refused.');

    toasts.clear();
    expect(toasts.toasts()).toEqual([]);
  });
});
