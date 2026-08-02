import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderService } from '../../../core/services/order';
import { Order } from '../../../core/models/order.model';

@Component({
  selector: 'app-orders',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './orders.html',
  styleUrl: './orders.scss',
})
export class Orders implements OnInit {
  private orderService = inject(OrderService);
  private cdr = inject(ChangeDetectorRef);

  orders: Order[] = [];
  filtered: Order[] = [];
  loading = true;
  search = '';
  activeStatus = 'All';

  statuses = ['All', 'Pending', 'Shipped', 'Delivered', 'Cancelled'];

  ngOnInit() {
    this.orderService.getOrders().subscribe({
      next: (res) => {
        this.orders = res;
        this.filtered = res;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilter() {
    this.filtered = this.orders.filter(o => {
      const matchStatus = this.activeStatus === 'All' || o.status === this.activeStatus;
      const matchSearch = `${o.shippingAddress.firstName} ${o.shippingAddress.lastName}`.toLowerCase().includes(this.search.toLowerCase())
        || o.id.toLowerCase().includes(this.search.toLowerCase());
      return matchStatus && matchSearch;
    });
    this.cdr.detectChanges();
  }

  setStatus(status: string) {
    this.activeStatus = status;
    this.applyFilter();
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
