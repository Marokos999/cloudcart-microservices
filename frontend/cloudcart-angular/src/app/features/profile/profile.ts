import { Component, inject, OnInit, ChangeDetectorRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { OrderService } from '../../core/services/order';
import { CatalogService } from '../../core/services/catalog';
import { Order } from '../../core/models/order.model';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, RouterLink],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
export class Profile implements OnInit {
  auth = inject(AuthService);
  private orderService = inject(OrderService);
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);

  profile = this.auth.profile;
  fullName = this.auth.fullName;

  orders: Order[] = [];
  loading = true;
  expandedOrderId = signal<string | null>(null);
  activeTab: 'orders' | 'account' = 'orders';
  avatarUrl = signal<string | null>(null);
  uploadingAvatar = false;

  get username(): string {
    return this.auth.getUsername() ?? '';
  }

  get roles(): string[] {
    return this.auth.getRoles();
  }

  get totalSpent(): number {
    return this.orders.reduce((sum, o) => sum + o.totalPrice, 0);
  }

  get activeOrders(): number {
    return this.orders.filter(o => o.status === 'Pending' || o.status === 'Processing' || o.status === 'Shipped').length;
  }

  ngOnInit() {
    const customerId = this.getCustomerId();
    const saved = localStorage.getItem(`avatar_${customerId}`);
    if (saved) this.avatarUrl.set(saved);
    this.orderService.getOrdersByCustomer(customerId).subscribe({
      next: (res) => {
        this.orders = res.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        this.loading = false;
        this.cdr.detectChanges();
        this.enrichOrderItems();
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); },
    });
  }

  getCustomerId(): string {
    return this.auth.getCustomerId();
  }

  toggleOrder(id: string) {
    this.expandedOrderId.set(this.expandedOrderId() === id ? null : id);
  }

  statusClass(status: string): string {
    return ({
      'Pending':    'badge-warning',
      'Processing': 'badge-processing',
      'Shipped':    'badge-info',
      'Delivered':  'badge-success',
      'Cancelled':  'badge-danger',
    } as Record<string, string>)[status] ?? '';
  }

  statusIcon(status: string): string {
    return ({ 'Pending': '🕐', 'Processing': '⚙️', 'Shipped': '🚚', 'Delivered': '✅', 'Cancelled': '✕' } as Record<string, string>)[status] ?? '•';
  }

  private enrichOrderItems() {
    const ids = [...new Set(this.orders.flatMap(o => o.orderItems.map(i => i.productId)))];
    ids.forEach(id => {
      this.catalogService.getProductById(id).subscribe({
        next: (p) => {
          this.orders.forEach(o => o.orderItems.forEach(item => {
            if (item.productId === id) {
              item.productName = p.name;
              item.imageFile = p.images?.[0] || p.imageFile || '';
            }
          }));
          this.cdr.detectChanges();
        },
        error: () => {}
      });
    });
  }

  onAvatarSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const userId = this.getCustomerId();
    this.uploadingAvatar = true;
    this.catalogService.uploadAvatar(userId, file).subscribe({
      next: (res) => {
        this.avatarUrl.set(res.avatarUrl);
        localStorage.setItem(`avatar_${userId}`, res.avatarUrl);
        this.uploadingAvatar = false;
        this.cdr.detectChanges();
      },
      error: () => { this.uploadingAvatar = false; this.cdr.detectChanges(); }
    });
  }

  initials(): string {
    const name = this.fullName();
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2) || '?';
  }
}
