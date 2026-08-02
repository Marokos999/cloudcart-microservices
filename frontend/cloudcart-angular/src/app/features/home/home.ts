import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CatalogService } from '../../core/services/catalog';
import { Product } from '../../core/models/product.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { timeout } from 'rxjs/operators';
import { BasketStore } from '../../core/services/basket-store';
import { ToastService } from '../../core/services/toast';

const DIRS = ['from-left', 'from-right', 'from-top', 'from-bottom'];

@Component({
  selector: 'app-home',
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements OnInit, OnDestroy {
  private catalogService = inject(CatalogService);
  private basketStore = inject(BasketStore);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  products: Product[] = [];
  loading = true;
  error = '';
  newsletterEmail = '';
  newsletterDone = false;

  imgIdx: { [id: string]: number } = {};
  imgAnim: { [id: string]: string } = {};
  private interval: ReturnType<typeof setInterval> | null = null;

  getImgs(p: Product): string[] {
    const arr = p.images?.length ? p.images : [];
    return arr.length ? arr : (p.imageFile ? [p.imageFile] : []);
  }

  getCurrentImg(p: Product): string {
    return this.getImgs(p)[this.imgIdx[p.id] ?? 0] ?? '';
  }

  getDir(i: number): string { return DIRS[i % 4]; }

  private startCycling() {
    this.interval = setInterval(() => {
      this.products.forEach((p, i) => {
        const imgs = this.getImgs(p);
        if (imgs.length < 2) return;
        this.imgAnim[p.id] = '';
        setTimeout(() => {
          this.imgIdx[p.id] = ((this.imgIdx[p.id] ?? 0) + 1) % imgs.length;
          this.imgAnim[p.id] = this.getDir(i);
          this.cdr.detectChanges();
        }, 20);
      });
      this.cdr.detectChanges();
    }, 3000);
  }

  ngOnDestroy() { if (this.interval) clearInterval(this.interval); }

  onSubscribe() {
    if (!this.newsletterEmail.includes('@')) return;
    this.newsletterDone = true;
    this.toast.show('Subscribed! Welcome to the loop.');
    this.newsletterEmail = '';
  }

  addToCart(event: Event, product: Product) {
    event.stopPropagation();
    this.basketStore.addItem(product);
    this.toast.show(`${product.name} added to cart`);
  }

  ngOnInit() {
    this.catalogService.getProducts().pipe(timeout(5000)).subscribe({
      next: (res) => {
        this.products = res.data.slice(0, 8);
        this.products.forEach(p => { this.imgIdx[p.id] = 0; this.imgAnim[p.id] = ''; });
        this.loading = false;
        this.cdr.detectChanges();
        this.startCycling();
      },
      error: (err) => {
        this.error = err.name === 'TimeoutError' ? 'Could not reach the server. Please try again.' : 'Failed to load products.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
