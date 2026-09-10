import { NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-avatar-stack',
  imports: [NgOptimizedImage],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="stack">
      <p class="sr-only">{{ count() }} {{ label() }}</p>
      @for (avatar of avatars(); track avatar) {
        <span class="stack__avatar">
          <img [ngSrc]="avatar" width="96" height="96" alt="" />
        </span>
      }
      <span class="stack__count" aria-hidden="true">{{ count() }}</span>
    </div>
  `,
  styles: `
    :host {
      display: block;
    }

    .stack {
      --avatar-size: clamp(2rem, min(3.4vw, 4.6vh), 2.9rem);
      --avatar-overlap: calc(var(--avatar-size) * -0.19);

      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .stack__avatar,
    .stack__count {
      width: var(--avatar-size);
      height: var(--avatar-size);
      border-radius: 50%;
    }

    .stack__avatar {
      overflow: hidden;
      border: 2px solid rgba(255, 255, 255, 0.85);
      box-shadow: 0 6px 18px rgba(72, 50, 26, 0.18);
    }

    .stack__avatar + .stack__avatar {
      margin-top: var(--avatar-overlap);
    }

    .stack__avatar img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .stack__count {
      display: grid;
      place-items: center;
      margin-top: var(--avatar-overlap);
      background: var(--STOR-ink);
      color: #fff;
      font-size: 0.8125rem;
      font-weight: 600;
    }

    @media (max-width: 899px) {
      .stack {
        flex-direction: row;
      }

      .stack__avatar + .stack__avatar,
      .stack__count {
        margin-top: 0;
        margin-inline-start: var(--avatar-overlap);
      }
    }
  `,
})
export class AvatarStack {
  readonly avatars = input.required<readonly string[]>();
  readonly count = input.required<string>();
  readonly label = input('happy customers');
}
