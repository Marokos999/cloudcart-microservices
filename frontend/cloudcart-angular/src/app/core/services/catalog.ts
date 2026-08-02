import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product, ProductsResponse } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private http = inject(HttpClient);
  private apiUrl = '/api/catalog';

  getProducts(page = 1, pageSize = 20): Observable<ProductsResponse> {
    return this.http.get<ProductsResponse>(`${this.apiUrl}/products?page=${page}&pageSize=${pageSize}`);
  }

  getProductsByCategory(category: string): Observable<ProductsResponse> {
    return this.http.get<ProductsResponse>(`${this.apiUrl}/products/category/${encodeURIComponent(category)}`);
  }

  getProductById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/products/${id}`);
  }

  uploadProductImage(id: string, file: File): Observable<{ imageUrl: string }> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ imageUrl: string }>(`${this.apiUrl}/products/${id}/image`, form);
  }

  createProduct(product: Omit<Product, 'id'>): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/products`, product);
  }

  updateProduct(product: Product): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/products/${product.id}`, product);
  }

  deleteProduct(id: string): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/products/${id}`);
  }

  uploadAvatar(userId: string, file: File): Observable<{ avatarUrl: string }> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ avatarUrl: string }>(`${this.apiUrl}/avatar/${userId}`, form);
  }
}
