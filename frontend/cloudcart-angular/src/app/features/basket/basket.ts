import { Component, inject, computed, signal, effect } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { BasketItem } from '../../core/models/basket.model';
import { BasketStore } from '../../core/services/basket-store';
import { DiscountService } from '../../core/services/discount';

@Component({
  selector: 'app-basket',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './basket.html',
  styleUrl: './basket.scss',
})
export class Basket {
  private basketStore = inject(BasketStore);
  private discountService = inject(DiscountService);

  items = this.basketStore.items;
  discounts = signal<Record<string, number>>({});

  couponCode = '';
  couponError = '';
  appliedCoupon = signal<{ code: string; amount: number; description: string } | null>(null);
  applying = false;

  total = computed(() => this.items().reduce((sum, i) => sum + i.price * i.quantity, 0));
  totalQuantity = computed(() => this.items().reduce((sum, i) => sum + i.quantity, 0));
  discountTotal = computed(() =>
    this.items().reduce((sum, i) => sum + (this.discounts()[i.productName] ?? 0) * i.quantity, 0)
  );
  couponDiscount = computed(() => this.appliedCoupon()?.amount ?? 0);
  finalTotal = computed(() => Math.max(0, this.total() - this.discountTotal() - this.couponDiscount()));

  constructor() {
    effect(() => {
      const names = [...new Set(this.items().map(i => i.productName))];
      if (names.length === 0) {
        this.discounts.set({});
        return;
      }

      forkJoin(
        names.map(name =>
          this.discountService.getCouponForProduct(name).pipe(
            map(c => ({ name, amount: c.amount })),
            catchError(() => of({ name, amount: 0 }))
          )
        )
      ).subscribe(results => {
        const next: Record<string, number> = {};
        for (const r of results) {
          if (r.amount > 0) next[r.name] = r.amount;
        }
        this.discounts.set(next);
      });
    });
  }

  increment(item: BasketItem) {
    const items = this.items().map(i =>
      i.productId === item.productId ? { ...i, quantity: i.quantity + 1 } : i
    );
    this.basketStore.save(items);
  }

  decrement(item: BasketItem) {
    const items = item.quantity > 1
      ? this.items().map(i => i.productId === item.productId ? { ...i, quantity: i.quantity - 1 } : i)
      : this.items().filter(i => i.productId !== item.productId);
    this.basketStore.save(items);
  }

  remove(item: BasketItem) {
    this.basketStore.save(this.items().filter(i => i.productId !== item.productId));
  }

  clear() {
    this.basketStore.clear();
    this.removeCoupon();
  }

  confirmClear() {
    if (confirm('Clear your entire cart?')) this.clear();
  }

  applyCoupon() {
    const code = this.couponCode.trim().toUpperCase();
    if (!code) return;

    this.applying = true;
    this.couponError = '';

    this.discountService.getCouponByCode(code).subscribe({
      next: (coupon) => {
        this.appliedCoupon.set({ code: coupon.code, amount: coupon.amount, description: coupon.description });
        this.couponCode = '';
        this.applying = false;
      },
      error: () => {
        this.couponError = 'Invalid coupon code.';
        this.applying = false;
      }
    });
  }

  removeCoupon() {
    this.appliedCoupon.set(null);
    this.couponError = '';
  }
}
