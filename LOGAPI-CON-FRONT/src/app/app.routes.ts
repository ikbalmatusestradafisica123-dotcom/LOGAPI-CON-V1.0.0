import { Routes } from '@angular/router';

import { authGuard } from './core/guards/guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/pages/login-page/login-page')
        .then((component) => component.LoginPage),
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard')
        .then((component) => component.Dashboard),
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orders/pages/orders-page/orders-page')
      .then((component) => component.OrdersPage),
  },
  {
    path: 'routes',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/routes/pages/routes-page/routes-page')
        .then((component) => component.RoutesPage),
  },
  {
    path: 'users',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/users/pages/users-page/users-page')
        .then((component) => component.UsersPage),
  },
  {
    path: 'vehicles',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/vehicles/pages/vehicles-page/vehicles-page')
        .then((component) => component.VehiclesPage),
  },
  {
    path: 'tracking',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/tracking/pages/tracking-page/tracking-page')
        .then((component) => component.TrackingPage),
  },
  {
    path: 'loads',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/loads/pages/loads-page/loads-page')
        .then((component) => component.LoadsPage),
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];
