import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CatalogService } from '../../../core/services/catalog';
import { Product } from '../../../core/models/product.model';
import { BasketStore } from '../../../core/services/basket-store';
import { ToastService } from '../../../core/services/toast';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
})
export class ProductDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);
  private basketStore = inject(BasketStore);
  private toast = inject(ToastService);

  product: Product | null = null;
  loading = true;
  activeIdx = 0;

  get allImages(): string[] {
    if (!this.product) return [];
    const imgs = this.product.images?.length ? this.product.images : [];
    if (this.product.imageFile && !imgs.includes(this.product.imageFile)) {
      return [this.product.imageFile, ...imgs];
    }
    return imgs.length ? imgs : [];
  }

  setImage(idx: number) { this.activeIdx = idx; }

  prev() { this.activeIdx = (this.activeIdx - 1 + this.allImages.length) % this.allImages.length; }
  next() { this.activeIdx = (this.activeIdx + 1) % this.allImages.length; }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.catalogService.getProductById(id).subscribe({
      next: (res) => {
        this.product = res;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  addToCart() {
    this.basketStore.addItem(this.product!);
    this.toast.show(`${this.product!.name} added to cart`);
  }
}
