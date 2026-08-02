import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Basket } from '../models/basket.model';

@Injectable({ providedIn: 'root' })
export class BasketService {
  private http = inject(HttpClient);
  private apiUrl = '/api/basket';

  getBasket(userName: string): Observable<Basket> {
    return this.http.get<Basket>(`${this.apiUrl}/basket/${userName}`);
  }

  storeBasket(basket: Basket): Observable<Basket> {
    return this.http.post<Basket>(`${this.apiUrl}/basket`, basket);
  }

  deleteBasket(userName: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/basket/${userName}`);
  }

  checkout(request: {
    userName: string; customerId: string;
    firstName: string; lastName: string; emailAddress: string;
    addressLine: string; country: string; state: string; zipCode: string;
    cardName: string; cardNumber: string; expiration: string; cvv: string;
    paymentMethod: number;
  }): Observable<{ isSuccess: boolean }> {
    return this.http.post<{ isSuccess: boolean }>(`${this.apiUrl}/checkout`, request);
  }
}
