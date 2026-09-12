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
    path: '**',
    redirectTo: '',
  },
];
