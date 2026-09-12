import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '@app/core/services/common/auth-store';

/**
 * Keeps the pages that only exist for a signed-in account - the cart, for one -
 * out of reach of visitors with no token. The API enforces this too; the guard
 * only saves them a page that could not load anything.
 *
 * The attempted URL rides along, so signing in lands the shopper where they were
 * headed rather than back at the top of the shop.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthStore);
  const router = inject(Router);

  if (auth.isSignedIn()) {
    return true;
  }

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
