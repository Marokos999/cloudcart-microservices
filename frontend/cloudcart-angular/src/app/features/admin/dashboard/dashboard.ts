import { Component, inject, ChangeDetectorRef, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../../core/services/order';
import { CatalogService } from '../../../core/services/catalog';

interface Stat {
  label: string;
  value: string;
  sub: string;
  color: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private orderService = inject(OrderService);
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);

  stats: Stat[] = [];
  orders: any[] = [];
  loading = true;
  private productCount = 0;
  private orderCount = 0;
  private revenue = 0;
  private pending = 0;

  private updateStats() {
    this.stats = [
      { label: 'Total Products', value: String(this.productCount), sub: 'in catalog', color: 'var(--primary)' },
      { label: 'Total Orders', value: this.orderCount ? String(this.orderCount) : '—', sub: 'all time', color: 'var(--cyan)' },
      { label: 'Revenue', value: this.revenue ? `$${this.revenue.toFixed(0)}` : '—', sub: 'all time', color: 'var(--success)' },
      { label: 'Pending', value: this.orderCount ? String(this.pending) : '—', sub: 'awaiting shipment', color: 'var(--warning)' },
    ];
  }

  ngOnInit() {
    this.catalogService.getProducts().subscribe({
      next: (res) => {
        this.productCount = res.total;
        this.updateStats();
        this.cdr.detectChanges();
      },
      error: () => this.cdr.detectChanges()
    });

    this.orderService.getOrders().subscribe({
      next: (res) => {
        this.orders = res.slice(0, 10);
        this.orderCount = res.length;
        this.revenue = res.reduce((s, o) => s + o.totalPrice, 0);
        this.pending = res.filter(o => o.status === 'Pending').length;
        this.updateStats();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.updateStats();
        this.cdr.detectChanges();
      }
    });
  }

  statusClass(status: string): string {
    return {
      'Pending': 'badge-warning',
      'Shipped': 'badge-info',
      'Delivered': 'badge-success',
      'Cancelled': 'badge-danger',
    }[status] ?? '';
  }
}
