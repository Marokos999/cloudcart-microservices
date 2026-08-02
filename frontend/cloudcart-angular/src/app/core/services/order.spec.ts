import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { OrderService } from './order';
import { Order } from '../models/order.model';

const mockOrder: Order = {
  id: 'ord-1',
  customerId: 'cust-1',
  orderName: 'ORD_001',
  shippingAddress: {
    firstName: 'John', lastName: 'Doe', emailAddress: 'john@example.com',
    addressLine: '123 Main St', country: 'USA', state: 'NY', zipCode: '10001',
  },
  billingAddress: {
    firstName: 'John', lastName: 'Doe', emailAddress: 'john@example.com',
    addressLine: '123 Main St', country: 'USA', state: 'NY', zipCode: '10001',
  },
  status: 'Pending',
  orderItems: [{ orderId: 'ord-1', productId: 'p1', quantity: 1, price: 499.99 }],
  totalPrice: 499.99,
  createdAt: '2026-01-01T00:00:00Z',
};

describe('OrderService', () => {
  let service: OrderService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(OrderService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should fetch all orders', () => {
    service.getOrders().subscribe(orders => {
      expect(orders).toHaveLength(1);
      expect(orders[0].orderName).toBe('ORD_001');
    });

    http.expectOne('/api/ordering/orders').flush([mockOrder]);
  });

  it('should fetch order by id', () => {
    service.getOrderById('ord-1').subscribe(order => {
      expect(order.id).toBe('ord-1');
    });

    http.expectOne('/api/ordering/orders/ord-1').flush(mockOrder);
  });

  it('should fetch orders by customer', () => {
    service.getOrdersByCustomer('cust-1').subscribe(orders => {
      expect(orders).toHaveLength(1);
      expect(orders[0].customerId).toBe('cust-1');
    });

    http.expectOne('/api/ordering/orders/customer/cust-1').flush([mockOrder]);
  });

  it('should create order and return id', () => {
    service.createOrder({ orderName: 'ORD_NEW' }).subscribe(res => {
      expect(res.id).toBe('new-id');
    });

    const req = http.expectOne('/api/ordering/orders');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toMatchObject({ orderName: 'ORD_NEW' });
    req.flush({ id: 'new-id' });
  });
});
