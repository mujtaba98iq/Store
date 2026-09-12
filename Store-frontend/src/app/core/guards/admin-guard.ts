import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '@app/core/services/common/auth-store';

/**
 * Keeps the management pages out of reach of visitors whose token carries no Admin
 * role. The API enforces this too - the guard only saves them a page of 403s.
 */
export const adminGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthStore);
  const router = inject(Router);

  if (auth.isAdmin()) {
    return true;
  }

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
