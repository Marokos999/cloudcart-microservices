import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface InventoryItem {
  id: string;
  productId: string;
  quantity: number;
  availableQuantity: number;
  reservedQuantity: number;
}

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private http = inject(HttpClient);
  private apiUrl = '/api/inventory';

  getInventoryItems(): Observable<InventoryItem[]> {
    return this.http.get<InventoryItem[]>(`${this.apiUrl}/inventory`);
  }

  getByProductId(productId: string): Observable<InventoryItem> {
    return this.http.get<InventoryItem>(`${this.apiUrl}/inventory/product/${productId}`);
  }

  getById(id: string): Observable<InventoryItem> {
    return this.http.get<InventoryItem>(`${this.apiUrl}/inventory/${id}`);
  }

  restock(id: string, addQuantity: number, currentTotal: number): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/inventory/${id}`, { quantity: currentTotal + addQuantity });
  }

  create(productId: string, productName: string, quantity: number): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/inventory`, { productId, productName, quantity });
  }
}
