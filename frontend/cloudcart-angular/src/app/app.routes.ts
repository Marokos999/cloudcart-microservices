import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/home/home').then(m => m.Home) },
  { path: 'products', loadComponent: () => import('./features/products/product-list/product-list').then(m => m.ProductList) },
  { path: 'products/:id', loadComponent: () => import('./features/products/product-detail/product-detail').then(m => m.ProductDetail) },
  { path: 'basket', loadComponent: () => import('./features/basket/basket').then(m => m.Basket) },
  { path: 'checkout', canActivate: [authGuard], loadComponent: () => import('./features/checkout/checkout').then(m => m.Checkout) },
  { path: 'admin', canActivate: [authGuard], loadComponent: () => import('./features/admin/dashboard/dashboard').then(m => m.Dashboard) },
  { path: 'admin/orders', canActivate: [authGuard], loadComponent: () => import('./features/admin/orders/orders').then(m => m.Orders) },
  { path: 'admin/orders/:id', canActivate: [authGuard], loadComponent: () => import('./features/admin/orders/order-detail/order-detail').then(m => m.OrderDetail) },
  { path: 'admin/products', canActivate: [authGuard], loadComponent: () => import('./features/admin/products/products').then(m => m.Products) },
  { path: 'admin/coupons', canActivate: [authGuard], loadComponent: () => import('./features/admin/coupons/coupons').then(m => m.Coupons) },
  { path: 'inventory', canActivate: [authGuard], loadComponent: () => import('./features/inventory/dashboard/dashboard').then(m => m.Dashboard) },
  { path: 'inventory/stock', canActivate: [authGuard], loadComponent: () => import('./features/inventory/stock/stock').then(m => m.Stock) },
  { path: 'inventory/stock/:productId', canActivate: [authGuard], loadComponent: () => import('./features/inventory/detail/detail').then(m => m.InventoryDetail) },
  { path: 'profile', canActivate: [authGuard], loadComponent: () => import('./features/profile/profile').then(m => m.Profile) },
  { path: 'about', loadComponent: () => import('./features/about/about').then(m => m.About) },
  { path: 'contact', loadComponent: () => import('./features/contact/contact').then(m => m.Contact) },
  { path: '**', redirectTo: '' }
];
