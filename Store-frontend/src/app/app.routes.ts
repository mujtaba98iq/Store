import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    title: 'STOR — Nurture Your Glow',
    loadComponent: () => import('./features/home/home').then((m) => m.Home),
  },
  {
    path: 'login',
    title: 'STOR — Sign In',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    // The catalogue reads are anonymous; only the admin controls need a token.
    path: 'products',
    title: 'STOR — Shop the Collection',
    loadComponent: () => import('./features/products/products').then((m) => m.Products),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
