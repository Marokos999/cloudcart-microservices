import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { InventoryService, InventoryItem } from '../../../core/services/inventory';
import { CatalogService } from '../../../core/services/catalog';
import { Product } from '../../../core/models/product.model';

interface StockItem {
  product: Product;
  inventory: InventoryItem;
  statusLabel: string;
  statusClass: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private inventoryService = inject(InventoryService);
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);

  items: StockItem[] = [];
  loading = true;

  get totalItems(): number { return this.items.length; }
  get lowStock(): number { return this.items.filter(i => i.inventory.availableQuantity <= 5).length; }
  get outOfStock(): number { return this.items.filter(i => i.inventory.availableQuantity === 0).length; }

  ngOnInit() {
    this.catalogService.getProducts().subscribe({
      next: (catalogRes) => {
        this.inventoryService.getInventoryItems().subscribe({
          next: (invRes) => {
            this.items = catalogRes.data.map(product => {
              const inventory = invRes.find(i => i.productId === product.id) ?? {
                id: '', productId: product.id, quantity: 0, availableQuantity: 0, reservedQuantity: 0
              };
              return { product, inventory, ...this.getStatus(inventory.availableQuantity) };
            });
            this.loading = false;
            this.cdr.detectChanges();
          },
          error: () => {
            this.items = catalogRes.data.map(product => ({
              product,
              inventory: { id: '', productId: product.id, quantity: 0, availableQuantity: 0, reservedQuantity: 0 },
              statusLabel: 'Unknown', statusClass: 'status-muted'
            }));
            this.loading = false;
            this.cdr.detectChanges();
          }
        });
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    });
  }

  private getStatus(qty: number): { statusLabel: string; statusClass: string } {
    if (qty === 0) return { statusLabel: 'Out of stock', statusClass: 'status-danger' };
    if (qty <= 5)  return { statusLabel: 'Low stock',    statusClass: 'status-warning' };
    return              { statusLabel: 'In stock',       statusClass: 'status-success' };
  }

  barWidth(qty: number): number {
    return Math.min((qty / 50) * 100, 100);
  }
}
