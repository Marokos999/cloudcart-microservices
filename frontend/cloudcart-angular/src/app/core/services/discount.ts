import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Coupon {
  id: number;
  productName: string;
  description: string;
  amount: number;
  code: string;
}

@Injectable({ providedIn: 'root' })
export class DiscountService {
  private http = inject(HttpClient);
  private apiUrl = '/api/discount';

  getCoupons(): Observable<Coupon[]> {
    return this.http.get<Coupon[]>(`${this.apiUrl}/coupons`);
  }

  getCouponForProduct(productName: string): Observable<Coupon> {
    return this.http.get<Coupon>(`${this.apiUrl}/coupons/product/${encodeURIComponent(productName)}`);
  }

  getCouponByCode(code: string): Observable<Coupon> {
    return this.http.get<Coupon>(`${this.apiUrl}/coupons/code/${encodeURIComponent(code)}`);
  }

  createCoupon(coupon: Omit<Coupon, 'id'>): Observable<Coupon> {
    return this.http.post<Coupon>(`${this.apiUrl}/coupons`, coupon);
  }

  updateCoupon(coupon: Coupon): Observable<Coupon> {
    return this.http.put<Coupon>(`${this.apiUrl}/coupons/${coupon.id}`, coupon);
  }

  deleteCoupon(id: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/coupons/${id}`);
  }
}
