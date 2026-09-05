import { Routes } from '@angular/router';
import { authGuard, unauthGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register.component').then(c => c.RegisterComponent),
    canActivate: [unauthGuard]
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(c => c.LoginComponent),
    canActivate: [unauthGuard]
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(c => c.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'products',
    loadComponent: () => import('./features/products/products.component').then(c => c.ProductsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'shopping-lists',
    loadComponent: () => import('./features/shopping-lists/shopping-lists.component').then(c => c.ShoppingListsComponent),
    canActivate: [authGuard]
  },
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
