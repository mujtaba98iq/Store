import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { IsActiveMatchOptions, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthStore } from '../../core/auth/auth-store';
import { Session } from '../../core/auth/session';

interface NavItem {
  readonly label: string;
  readonly link: string;
  readonly fragment?: string;
  /** Empty for in-page anchors, which never own the active state. */
  readonly activeClass: string;
}

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styleUrl: './header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Header {
  private readonly session = inject(Session);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthStore);

  protected readonly isSignedIn = this.auth.isSignedIn;
  protected readonly email = this.auth.email;

  protected readonly exactMatch: IsActiveMatchOptions = {
    paths: 'exact',
    queryParams: 'ignored',
    fragment: 'ignored',
    matrixParams: 'ignored',
  };

  // Concept and Contact are still sections of the landing page.
  protected readonly navItems = signal<readonly NavItem[]>([
    { label: 'Products', link: '/products', activeClass: 'nav__link--active' },
    { label: 'Concept', link: '/', fragment: 'concept', activeClass: '' },
    { label: 'Contact Us', link: '/', fragment: 'contact', activeClass: '' },
  ]);

  protected signOut(): void {
    this.session.signOut();
    void this.router.navigate(['/']);
  }
}
