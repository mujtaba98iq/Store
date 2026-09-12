import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { IsActiveMatchOptions, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthStore } from '@app/core/services/common/auth-store';
import { CartStore } from '@app/core/services/common/cart-store';
import { SessionService } from '@app/core/services/common/session';

interface NavItem {
  readonly label: string;
  readonly link: string;
  readonly fragment?: string;
  /** Empty for in-page anchors, which never own the active state. */
  readonly activeClass: string;
  /** Links behind the admin guard, hidden from everyone who cannot follow them. */
  readonly adminOnly?: boolean;
}

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styleUrl: './header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Header {
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthStore);
  private readonly cart = inject(CartStore);

  protected readonly isSignedIn = this.auth.isSignedIn;
  protected readonly email = this.auth.email;

  /**
   * Units in the cart, not lines: the badge answers "how many things are in my
   * bag", and two bottles of one serum is two things.
   */
  protected readonly cartCount = this.cart.unitCount;

  /** The count is drawn, so it has to be said as well. */
  protected readonly cartLabel = computed(() => {
    const count = this.cartCount();
    if (!this.isSignedIn()) {
      return 'Shopping bag';
    }

    return count ? `Shopping bag, ${count} item${count === 1 ? '' : 's'}` : 'Shopping bag, empty';
  });

  protected readonly exactMatch: IsActiveMatchOptions = {
    paths: 'exact',
    queryParams: 'ignored',
    fragment: 'ignored',
    matrixParams: 'ignored',
  };

  // Concept and Contact are still sections of the landing page.
  private readonly allNavItems = signal<readonly NavItem[]>([
    { label: 'Products', link: '/products', activeClass: 'nav__link--active' },
    {
      label: 'Categories',
      link: '/categories',
      activeClass: 'nav__link--active',
      adminOnly: true,
    },
    {
      label: 'Variants',
      link: '/product-variants',
      activeClass: 'nav__link--active',
      adminOnly: true,
    },
    {
      label: 'Inventories',
      link: '/inventories',
      activeClass: 'nav__link--active',
      adminOnly: true,
    },
    {
      label: 'Users',
      link: '/users',
      activeClass: 'nav__link--active',
      adminOnly: true,
    },
    { label: 'Concept', link: '/', fragment: 'concept', activeClass: '' },
    { label: 'Contact Us', link: '/', fragment: 'contact', activeClass: '' },
  ]);

  protected readonly navItems = computed(() =>
    this.allNavItems().filter((item) => !item.adminOnly || this.auth.isAdmin()),
  );

  protected signOut(): void {
    this.session.signOut();
    void this.router.navigate(['/']);
  }
}
