import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../../../core/services/order';
import { Order } from '../../../../core/models/order.model';

@Component({
  selector: 'app-order-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './order-detail.html',
  styleUrl: './order-detail.scss',
})
export class OrderDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);
  private cdr = inject(ChangeDetectorRef);

  order: Order | null = null;
  loading = true;

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.orderService.getOrderById(id).subscribe({
      next: (order) => {
        this.order = order;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    });
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

  sameAddress(): boolean {
    if (!this.order) return true;
    const s = this.order.shippingAddress;
    const b = this.order.billingAddress;
    return s.addressLine === b.addressLine && s.firstName === b.firstName;
  }
}
