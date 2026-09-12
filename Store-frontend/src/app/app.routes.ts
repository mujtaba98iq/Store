import { Routes } from '@angular/router';
import { adminGuard } from '@app/core/guards/admin-guard';

export const routes: Routes = [
  {
    path: '',
    title: 'STOR — Nurture Your Glow',
    loadComponent: () => import('./modules/home/pages/home/home').then((m) => m.Home),
  },
  {
    path: 'login',
    title: 'STOR — Sign In',
    loadComponent: () => import('./modules/auth/pages/login/login').then((m) => m.Login),
  },
  {
    // The catalogue reads are anonymous; only the admin controls need a token.
    path: 'products',
    title: 'STOR — Shop the Collection',
    loadComponent: () =>
      import('./modules/products/pages/products/products').then((m) => m.Products),
  },
  {
    // Every control on this page is an admin one, so the whole route is guarded.
    path: 'categories',
    title: 'STOR — Categories',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./modules/categories/pages/categories/categories').then((m) => m.Categories),
  },
  {
    // Account administration, top to bottom. The API puts the whole users resource
    // behind the Admin role, so the guard on this route matches it.
    path: 'users',
    title: 'STOR — Users',
    canActivate: [adminGuard],
    loadComponent: () => import('./modules/users/pages/users/users').then((m) => m.Users),
  },
  {
    // Variants are readable by any signed-in account, but every control here
    // writes, and the API puts all of those behind the Admin role.
    path: 'product-variants',
    title: 'STOR — Product Variants',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./modules/product-variants/pages/product-variants/product-variants').then(
        (m) => m.ProductVariants,
      ),
  },
  {
    // Stock levels are readable by any signed-in account, but every control here
    // writes, and the API puts all of those behind the Admin role.
    path: 'inventories',
    title: 'STOR — Inventories',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./modules/inventories/pages/inventories/inventories').then((m) => m.Inventories),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
