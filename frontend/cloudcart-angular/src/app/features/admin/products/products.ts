import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CatalogService } from '../../../core/services/catalog';
import { Product } from '../../../core/models/product.model';

@Component({
  selector: 'app-products',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './products.html',
  styleUrl: './products.scss',
})
export class Products implements OnInit {
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);

  products: Product[] = [];
  filtered: Product[] = [];
  loading = true;
  search = '';
  uploadingId: string | null = null;

  formOpen = false;
  editingId: string | null = null;
  saving = false;
  form = this.emptyForm();

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.catalogService.getProducts().subscribe({
      next: (res) => {
        this.products = res.data;
        this.applyFilter();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilter() {
    this.filtered = this.products.filter(p =>
      p.name.toLowerCase().includes(this.search.toLowerCase()) ||
      p.category.toLowerCase().includes(this.search.toLowerCase())
    );
    this.cdr.detectChanges();
  }

  private emptyForm() {
    return { name: '', category: '', description: '', imageFile: '', price: 0 };
  }

  openCreate() {
    this.editingId = null;
    this.form = this.emptyForm();
    this.formOpen = true;
  }

  openEdit(product: Product) {
    this.editingId = product.id;
    this.form = {
      name: product.name,
      category: product.category,
      description: product.description,
      imageFile: product.imageFile,
      price: product.price,
    };
    this.formOpen = true;
  }

  closeForm() {
    this.formOpen = false;
  }

  saveProduct() {
    if (!this.form.name || !this.form.category || this.form.price <= 0) return;

    this.saving = true;
    const done = () => {
      this.saving = false;
      this.formOpen = false;
      this.loadProducts();
    };
    const fail = () => {
      this.saving = false;
      this.cdr.detectChanges();
    };

    if (this.editingId) {
      this.catalogService.updateProduct({ id: this.editingId, ...this.form }).subscribe({
        next: done,
        error: fail,
      });
    } else {
      this.catalogService.createProduct(this.form).subscribe({
        next: done,
        error: fail,
      });
    }
  }

  deleteProduct(product: Product) {
    if (!confirm(`Delete "${product.name}"?`)) return;

    this.catalogService.deleteProduct(product.id).subscribe({
      next: () => this.loadProducts(),
    });
  }

  triggerUpload(input: HTMLInputElement) {
    input.value = '';
    input.click();
  }

  onFileSelected(productId: string, event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.uploadingId = productId;
    this.cdr.detectChanges();

    this.catalogService.uploadProductImage(productId, file).subscribe({
      next: (res) => {
        const product = this.products.find(p => p.id === productId);
        if (product) product.imageFile = res.imageUrl;
        this.uploadingId = null;
        this.cdr.detectChanges();
      },
      error: () => {
        this.uploadingId = null;
        this.cdr.detectChanges();
      }
    });
  }
}
