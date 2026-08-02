import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { PaymentService } from './payment';

describe('PaymentService', () => {
  let service: PaymentService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(PaymentService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should call create-intent endpoint with correct body', () => {
    service.createIntent(499.99, 'cust-1', 'ORD_001').subscribe(res => {
      expect(res.clientSecret).toBe('cs_test_abc');
      expect(res.paymentIntentId).toBe('pi_test_123');
    });

    const req = http.expectOne('/api/payment/create-intent');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ amount: 499.99, customerId: 'cust-1', orderName: 'ORD_001' });
    req.flush({ clientSecret: 'cs_test_abc', paymentIntentId: 'pi_test_123' });
  });

  it('should pass the correct amount', () => {
    service.createIntent(1299.97, 'cust-2', 'ORD_002').subscribe();

    const req = http.expectOne('/api/payment/create-intent');
    expect(req.request.body.amount).toBe(1299.97);
    req.flush({ clientSecret: 'cs_test', paymentIntentId: 'pi_test' });
  });
});
