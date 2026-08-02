import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { loadStripe, Stripe, StripeElements } from '@stripe/stripe-js';
import { BasketStore } from '../../core/services/basket-store';
import { BasketService } from '../../core/services/basket';
import { ToastService } from '../../core/services/toast';
import { PaymentService } from '../../core/services/payment';
import { AuthService } from '../../core/services/auth';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-checkout',
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss',
})
export class Checkout implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private basketStore = inject(BasketStore);
  private basketService = inject(BasketService);
  private toast = inject(ToastService);
  private router = inject(Router);
  private paymentService = inject(PaymentService);
  private auth = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  items = this.basketStore.items;
  total = computed(() => this.items().reduce((sum, i) => sum + i.price * i.quantity, 0));

  submitting = false;
  stripeReady = false;
  stripeError = '';

  private stripe: Stripe | null = null;
  private elements: StripeElements | null = null;
  private clientSecret = '';

  form = this.fb.group({
    firstName: ['', Validators.required],
    lastName:  ['', Validators.required],
    email:     ['', [Validators.required, Validators.email]],
    address:   ['', Validators.required],
    city:      ['', Validators.required],
    zip:       ['', Validators.required],
    country:   ['', Validators.required],
  });

  get f() { return this.form.controls; }

  async ngOnInit() {
    if (this.items().length === 0) return;

    this.stripe = await loadStripe(environment.stripePk);
    if (!this.stripe) return;

    const customerId = this.getCustomerId();
    const orderName  = `ORD-${Date.now()}`;

    this.paymentService.createIntent(this.total(), customerId, orderName).subscribe({
      next: async (res) => {
        this.clientSecret = res.clientSecret;

        this.elements = this.stripe!.elements({
          clientSecret: res.clientSecret,
          appearance: {
            theme: 'night',
            variables: {
              colorPrimary: '#6c63ff',
              colorBackground: '#1a1a2e',
              colorText: '#e2e8f0',
              colorDanger: '#ef4444',
              fontFamily: 'Inter, sans-serif',
              borderRadius: '8px',
            },
          },
        });

        const paymentElement = this.elements.create('payment');
        paymentElement.mount('#stripe-payment-element');
        paymentElement.on('ready', () => {
          this.stripeReady = true;
          this.cdr.detectChanges();
        });
      },
      error: () => {
        this.stripeError = 'Could not load payment form. Try again.';
        this.cdr.detectChanges();
      },
    });
  }

  ngOnDestroy() {
    this.elements?.getElement('payment')?.destroy();
  }

  async submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    if (!this.stripe || !this.elements) return;

    this.submitting = true;
    this.stripeError = '';

    const v = this.form.value;

    const { error, paymentIntent } = await this.stripe.confirmPayment({
      elements: this.elements,
      confirmParams: { return_url: window.location.origin },
      redirect: 'if_required',
    });

    if (error) {
      this.stripeError = error.message ?? 'Payment failed.';
      this.submitting = false;
      this.cdr.detectChanges();
      return;
    }

    if (paymentIntent?.status === 'succeeded') {
      const userName = this.auth.getUsername() ?? 'guest';

      const basketPayload = {
        userName,
        items: this.items().map(i => ({
          productId:   i.productId,
          productName: i.productName,
          price:       i.price,
          quantity:    i.quantity,
          imageFile:   i.imageFile,
        })),
        totalPrice: this.total(),
      };

      this.basketService.storeBasket(basketPayload).pipe(
        switchMap(() => this.basketService.checkout({
          userName,
          customerId:    this.getCustomerId(),
          firstName:     v.firstName!,
          lastName:      v.lastName!,
          emailAddress:  v.email!,
          addressLine:   v.address!,
          country:       v.country!,
          state:         v.city!,
          zipCode:       v.zip!,
          cardName:      `${v.firstName} ${v.lastName}`,
          cardNumber:    paymentIntent.id.substring(0, 24),
          expiration:    '00/00',
          cvv:           '000',
          paymentMethod: 1,
        }))
      ).subscribe({
        next: () => {
          this.basketStore.clear();
          this.submitting = false;
          this.toast.show('Order placed successfully!');
          this.router.navigate(['/profile']);
        },
        error: () => {
          this.submitting = false;
          this.toast.show('Payment succeeded but order failed. Contact support.', 'error');
          this.cdr.detectChanges();
        },
      });
    }
  }

  private getCustomerId(): string {
    return this.auth.getCustomerId() || '00000000-0000-0000-0000-000000000001';
  }
}
