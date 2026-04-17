import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'produtos',
  },
  {
    path: 'produtos',
    loadComponent: () =>
      import('./pages/products-page/products-page').then((module) => module.ProductsPage),
  },
  {
    path: 'notas-fiscais',
    loadComponent: () =>
      import('./pages/invoices-page/invoices-page').then((module) => module.InvoicesPage),
  },
  {
    path: '**',
    redirectTo: 'produtos',
  },
];
