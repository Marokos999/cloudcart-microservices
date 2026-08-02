import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DiscountService, Coupon } from '../../../core/services/discount';
import { CatalogService } from '../../../core/services/catalog';

@Component({
  selector: 'app-coupons',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './coupons.html',
  styleUrl: './coupons.scss',
})
export class Coupons implements OnInit {
  private discountService = inject(DiscountService);
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);

  coupons: Coupon[] = [];
  productNames: string[] = [];
  loading = true;

  formOpen = false;
  editingId: number | null = null;
  saving = false;
  form = this.emptyForm();

  ngOnInit() {
    this.loadCoupons();
    this.catalogService.getProducts().subscribe({
      next: (res) => {
        this.productNames = res.data.map(p => p.name);
        this.cdr.detectChanges();
      },
    });
  }

  loadCoupons() {
    this.discountService.getCoupons().subscribe({
      next: (coupons) => {
        this.coupons = coupons;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  private emptyForm() {
    return { productName: '', description: '', amount: 0, code: '' };
  }

  openCreate() {
    this.editingId = null;
    this.form = this.emptyForm();
    this.formOpen = true;
  }

  openEdit(coupon: Coupon) {
    this.editingId = coupon.id;
    this.form = {
      productName: coupon.productName,
      description: coupon.description,
      amount: coupon.amount,
      code: coupon.code,
    };
    this.formOpen = true;
  }

  closeForm() {
    this.formOpen = false;
  }

  saveCoupon() {
    if (this.form.amount <= 0) return;
    if (!this.form.productName && !this.form.code) return;
    this.form.code = this.form.code.trim().toUpperCase();

    this.saving = true;
    const done = () => {
      this.saving = false;
      this.formOpen = false;
      this.loadCoupons();
    };
    const fail = () => {
      this.saving = false;
      this.cdr.detectChanges();
    };

    if (this.editingId) {
      this.discountService.updateCoupon({ id: this.editingId, ...this.form }).subscribe({
        next: done,
        error: fail,
      });
    } else {
      this.discountService.createCoupon(this.form).subscribe({
        next: done,
        error: fail,
      });
    }
  }

  deleteCoupon(coupon: Coupon) {
    if (!confirm(`Delete coupon for "${coupon.productName}"?`)) return;

    this.discountService.deleteCoupon(coupon.id).subscribe({
      next: () => this.loadCoupons(),
    });
  }
}
