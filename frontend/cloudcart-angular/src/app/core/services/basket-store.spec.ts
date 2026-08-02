import { TestBed } from '@angular/core/testing';
import { BasketStore } from './basket-store';

describe('BasketStore', () => {
  let store: BasketStore;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    store = TestBed.inject(BasketStore);
  });

  it('should start empty when localStorage is empty', () => {
    expect(store.items()).toEqual([]);
    expect(store.count()).toBe(0);
  });

  it('should add a new item', () => {
    store.addItem({ id: 'p1', name: 'PlayStation 5', price: 499.99, imageFile: 'ps5.jpg' });

    expect(store.items()).toHaveLength(1);
    expect(store.items()[0].productId).toBe('p1');
    expect(store.items()[0].quantity).toBe(1);
  });

  it('should increment quantity when same item is added twice', () => {
    const product = { id: 'p1', name: 'Xbox', price: 449.99, imageFile: 'xbox.jpg' };
    store.addItem(product);
    store.addItem(product);

    expect(store.items()).toHaveLength(1);
    expect(store.items()[0].quantity).toBe(2);
  });

  it('should add multiple distinct items', () => {
    store.addItem({ id: 'p1', name: 'PS5', price: 499.99, imageFile: 'ps5.jpg' });
    store.addItem({ id: 'p2', name: 'Xbox', price: 449.99, imageFile: 'xbox.jpg' });

    expect(store.items()).toHaveLength(2);
  });

  it('should compute count as sum of quantities', () => {
    store.addItem({ id: 'p1', name: 'PS5', price: 499.99, imageFile: 'ps5.jpg' });
    store.addItem({ id: 'p1', name: 'PS5', price: 499.99, imageFile: 'ps5.jpg' });
    store.addItem({ id: 'p2', name: 'Xbox', price: 449.99, imageFile: 'xbox.jpg' });

    expect(store.count()).toBe(3);
  });

  it('should clear all items', () => {
    store.addItem({ id: 'p1', name: 'PS5', price: 499.99, imageFile: 'ps5.jpg' });
    store.clear();

    expect(store.items()).toEqual([]);
    expect(store.count()).toBe(0);
  });

  it('should persist items to localStorage', () => {
    store.addItem({ id: 'p1', name: 'PS5', price: 499.99, imageFile: 'ps5.jpg' });

    const stored = JSON.parse(localStorage.getItem('basket') ?? '[]');
    expect(stored).toHaveLength(1);
    expect(stored[0].productId).toBe('p1');
  });

  it('should load items from localStorage on init', () => {
    localStorage.setItem('basket', JSON.stringify([
      { productId: 'p1', productName: 'PS5', price: 499.99, quantity: 2, imageFile: 'ps5.jpg' }
    ]));

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({});
    const freshStore = TestBed.inject(BasketStore);

    expect(freshStore.items()).toHaveLength(1);
    expect(freshStore.items()[0].quantity).toBe(2);
  });

  it('should save custom items list', () => {
    store.save([
      { productId: 'p1', productName: 'PS5', price: 499.99, quantity: 3, imageFile: 'ps5.jpg' }
    ]);

    expect(store.items()[0].quantity).toBe(3);
  });
});
