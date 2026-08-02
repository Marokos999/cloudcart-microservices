import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InventoryService, InventoryItem } from '../../../core/services/inventory';
import { CatalogService } from '../../../core/services/catalog';
import { Product } from '../../../core/models/product.model';

interface StockRow {
  product: Product;
  inventory: InventoryItem;
}

@Component({
  selector: 'app-stock',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './stock.html',
  styleUrl: './stock.scss',
})
export class Stock implements OnInit {
  private inventoryService = inject(InventoryService);
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);

  rows: StockRow[] = [];
  filtered: StockRow[] = [];
  loading = true;
  search = '';

  ngOnInit() {
    this.catalogService.getProducts().subscribe({
      next: (catalogRes) => {
        this.inventoryService.getInventoryItems().subscribe({
          next: (invRes) => {
            this.rows = catalogRes.data.map(product => ({
              product,
              inventory: invRes.find(i => i.productId === product.id) ?? {
                id: '', productId: product.id, quantity: 0, availableQuantity: 0, reservedQuantity: 0
              }
            }));
            this.filtered = this.rows;
            this.loading = false;
            this.cdr.detectChanges();
          },
          error: () => {
            this.rows = catalogRes.data.map(product => ({
              product,
              inventory: { id: '', productId: product.id, quantity: 0, availableQuantity: 0, reservedQuantity: 0 }
            }));
            this.filtered = this.rows;
            this.loading = false;
            this.cdr.detectChanges();
          }
        });
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    });
  }

  applyFilter() {
    this.filtered = this.rows.filter(r =>
      r.product.name.toLowerCase().includes(this.search.toLowerCase())
    );
    this.cdr.detectChanges();
  }
}
