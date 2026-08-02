import { Injectable, signal, computed } from '@angular/core';
import { BasketItem } from '../models/basket.model';

@Injectable({ providedIn: 'root' })
export class BasketStore {
  private key = 'basket';
  private _items = signal<BasketItem[]>(this.load());

  readonly items = this._items.asReadonly();
  readonly count = computed(() => this._items().reduce((sum, i) => sum + i.quantity, 0));

  private load(): BasketItem[] {
    const stored = localStorage.getItem(this.key);
    return stored ? JSON.parse(stored) : [];
  }

  private persist(items: BasketItem[]) {
    localStorage.setItem(this.key, JSON.stringify(items));
    this._items.set(items);
  }

  getItems(): BasketItem[] {
    return this._items();
  }

  addItem(product: { id: string; name: string; price: number; imageFile: string }) {
    const items = [...this._items()];
    const existing = items.find(i => i.productId === product.id);
    if (existing) {
      existing.quantity++;
    } else {
      items.push({
        productId: product.id,
        productName: product.name,
        price: product.price,
        quantity: 1,
        imageFile: product.imageFile
      });
    }
    this.persist(items);
  }

  save(items: BasketItem[]) {
    this.persist(items);
  }

  clear() {
    localStorage.removeItem(this.key);
    this._items.set([]);
  }
}
