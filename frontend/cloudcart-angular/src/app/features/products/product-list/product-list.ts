import { Component, inject, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { CatalogService } from '../../../core/services/catalog';
import { Product } from '../../../core/models/product.model';
import { BasketStore } from '../../../core/services/basket-store';
import { ToastService } from '../../../core/services/toast';

const PAGE_SIZE = 20;
const DIRS = ['from-left', 'from-right', 'from-top', 'from-bottom'];

@Component({
  selector: 'app-product-list',
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
})

export class ProductList implements OnInit, OnDestroy {
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);
  private basketStore = inject(BasketStore);
  private router = inject(Router);
  private toast = inject(ToastService);

  allProducts: Product[] = [];
  filtered: Product[] = [];
  categories: string[] = ['All'];
  loading = true;
  search = '';
  activeCategory = 'All';

  imgIdx: { [id: string]: number } = {};
  imgAnim: { [id: string]: string } = {};
  private interval: ReturnType<typeof setInterval> | null = null;

  getImgs(p: Product): string[] {
    const list = p.images?.length ? p.images : [];
    if (p.imageFile && !list.includes(p.imageFile)) return [p.imageFile, ...list];
    return list.length ? list : (p.imageFile ? [p.imageFile] : []);
  }

  getCurrentImg(p: Product): string { return this.getImgs(p)[this.imgIdx[p.id] ?? 0] ?? ''; }
  getDir(i: number): string { return DIRS[i % 4]; }

  private startCycling() {
    this.interval = setInterval(() => {
      this.filtered.forEach((p, i) => {
        const imgs = this.getImgs(p);
        if (imgs.length < 2) return;
        const dir = this.getDir(i);
        this.imgAnim[p.id] = '';
        setTimeout(() => {
          const cur = this.imgIdx[p.id] ?? 0;
          this.imgIdx[p.id] = (cur + 1) % imgs.length;
          this.imgAnim[p.id] = dir;
          this.cdr.detectChanges();
        }, 20);
      });
      this.cdr.detectChanges();
    }, 3000);
  }

  ngOnDestroy() { if (this.interval) clearInterval(this.interval); }

  // pagination
  page = 1;
  pageSize = PAGE_SIZE;
  total = 0;

  get totalPages() { return Math.ceil(this.total / this.pageSize); }
  get pages() {
    const delta = 2;
    const range: number[] = [];
    for (let i = Math.max(1, this.page - delta); i <= Math.min(this.totalPages, this.page + delta); i++) {
      range.push(i);
    }
    return range;
  }

  goToProduct(id: string) { this.router.navigate(['/products', id]); }

  addToCart(event: Event, product: Product) {
    event.stopPropagation();
    this.basketStore.addItem(product);
    this.toast.show(`${product.name} added to cart`);
  }

  ngOnInit(): void { this.loadPage(1); }

  loadPage(p: number) {
    this.loading = true;
    this.page = p;
    this.activeCategory = 'All';
    this.search = '';
    this.catalogService.getProducts(p, this.pageSize).subscribe({
      next: (res) => {
        this.allProducts = res.data;
        this.filtered = res.data;
        this.total = res.total;
        this.categories = ['All', ...new Set(res.data.map(prod => prod.category))];
        this.filtered.forEach(p => { this.imgIdx[p.id] = 0; this.imgAnim[p.id] = ''; });
        this.loading = false;
        this.cdr.detectChanges();
        if (this.interval) clearInterval(this.interval);
        this.startCycling();
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    });
  }

  applyFilter() {
    const s = this.search.toLowerCase();
    this.filtered = this.allProducts.filter(p => {
      const matchCat = this.activeCategory === 'All' || p.category === this.activeCategory;
      return matchCat && p.name.toLowerCase().includes(s);
    });
  }

  setCategory(cat: string) {
    this.activeCategory = cat;
    this.search = '';
    this.filtered = cat === 'All'
      ? this.allProducts
      : this.allProducts.filter(p => p.category === cat);
    this.cdr.detectChanges();
  }

  goPage(p: number) {
    if (p < 1 || p > this.totalPages || p === this.page) return;
    this.loadPage(p);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
