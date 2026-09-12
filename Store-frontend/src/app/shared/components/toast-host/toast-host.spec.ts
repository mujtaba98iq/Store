import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToastService } from '@app/core/services/common/toast';
import { ToastHost } from './toast-host';

describe('ToastHost', () => {
  let fixture: ComponentFixture<ToastHost>;
  let element: HTMLElement;
  let toasts: ToastService;

  const rendered = () =>
    Array.from(element.querySelectorAll('.toast')).map((node) => ({
      message: node.querySelector('.toast__message')?.textContent?.trim(),
      role: node.getAttribute('role'),
      classes: node.className,
    }));

  const render = () => fixture.detectChanges();

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ToastHost] }).compileComponents();

    toasts = TestBed.inject(ToastService);
    fixture = TestBed.createComponent(ToastHost);
    element = fixture.nativeElement as HTMLElement;
    render();
  });

  afterEach(() => {
    toasts.clear();
  });

  it('renders nothing until something is announced', () => {
    expect(rendered()).toEqual([]);
  });

  it('marks the stack as a labelled live region', () => {
    expect(element.getAttribute('role')).toBe('region');
    expect(element.getAttribute('aria-label')).toBe('Notifications');
    expect(element.querySelector('.stack')?.getAttribute('aria-live')).toBe('polite');
  });

  it('gives each kind its own class so it reads at a glance', () => {
    toasts.success('Product created successfully.');
    toasts.info('Noted.');
    render();

    expect(rendered().map((toast) => toast.classes)).toEqual([
      expect.stringContaining('toast--info'),
      expect.stringContaining('toast--success'),
    ]);
  });

  it('interrupts for a failure and waits its turn for a confirmation', () => {
    toasts.error('Refused.');
    toasts.success('Saved.');
    toasts.warning('Careful.');
    toasts.info('Noted.');
    render();

    const byMessage = new Map(rendered().map((toast) => [toast.message, toast.role]));
    expect(byMessage.get('Refused.')).toBe('alert');
    expect(byMessage.get('Careful.')).toBe('alert');
    expect(byMessage.get('Saved.')).toBe('status');
    expect(byMessage.get('Noted.')).toBe('status');
  });

  it('dismisses from the keyboard as well as the pointer', () => {
    toasts.error('Refused.');
    render();

    const close = element.querySelector<HTMLButtonElement>('.toast__close');
    expect(close?.getAttribute('aria-label')).toBe('Dismiss notification');

    close!.click();
    render();

    // Marked on its way out; the service drops it once the animation is done.
    expect(rendered()[0].classes).toContain('toast--leaving');
  });

  it('closes on Escape from within the toast', () => {
    toasts.error('Refused.');
    render();

    element
      .querySelector('.toast')!
      .dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    render();

    expect(rendered()[0].classes).toContain('toast--leaving');
  });

  it('shows how many times a repeat arrived', () => {
    toasts.error('That item no longer exists.');
    toasts.error('That item no longer exists.');
    render();

    expect(rendered().length).toBe(1);
    expect(element.querySelector('.toast__repeats')?.textContent?.trim()).toBe('×2');
  });

  it('holds the countdown while the pointer is on the toast', () => {
    vi.useFakeTimers();
    try {
      toasts.success('Saved.');
      render();

      // The host is click-through, so the toast is what the pointer lands on.
      const toast = element.querySelector('.toast')!;

      toast.dispatchEvent(new PointerEvent('pointerenter'));
      vi.advanceTimersByTime(20_000);
      render();
      expect(rendered().length).toBe(1);

      toast.dispatchEvent(new PointerEvent('pointerleave'));
      vi.advanceTimersByTime(20_000);
      render();
      expect(rendered()).toEqual([]);
    } finally {
      vi.useRealTimers();
    }
  });

  it('holds the countdown while the dismiss button has focus', () => {
    vi.useFakeTimers();
    try {
      toasts.success('Saved.');
      render();

      element
        .querySelector('.toast__close')!
        .dispatchEvent(new FocusEvent('focusin', { bubbles: true }));
      vi.advanceTimersByTime(20_000);
      render();
      expect(rendered().length).toBe(1);
    } finally {
      vi.useRealTimers();
    }
  });
});
