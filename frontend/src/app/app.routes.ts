import { Routes } from '@angular/router';
import { authGuard, guestGuard, permissionGuard } from './core/auth/auth.guards';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell.component').then((m) => m.ShellComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'products' },
      {
        path: 'products',
        canActivate: [permissionGuard('Products.View')],
        loadComponent: () =>
          import('./features/products/product-list.component').then((m) => m.ProductListComponent),
      },
      {
        path: 'products/:id/stock',
        canActivate: [permissionGuard('Stock.Move')],
        loadComponent: () =>
          import('./features/stock/stock-operations.component').then(
            (m) => m.StockOperationsComponent,
          ),
      },
      {
        path: 'history',
        canActivate: [permissionGuard('Stock.ViewHistory')],
        loadComponent: () =>
          import('./features/history/history.component').then((m) => m.HistoryComponent),
      },
      {
        path: 'users',
        canActivate: [permissionGuard('Users.Manage')],
        loadComponent: () => import('./features/users/users.component').then((m) => m.UsersComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
