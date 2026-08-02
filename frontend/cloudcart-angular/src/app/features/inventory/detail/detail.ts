import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { InventoryService, InventoryItem } from '../../../core/services/inventory';
import { CatalogService } from '../../../core/services/catalog';
import { Product } from '../../../core/models/product.model';
import { ToastService } from '../../../core/services/toast';

@Component({
  selector: 'app-inventory-detail',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
})
export class InventoryDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private inventoryService = inject(InventoryService);
  private catalogService = inject(CatalogService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  product: Product | null = null;
  inventory: InventoryItem | null = null;
  loading = true;
  restockAmount = 0;
  createAmount = 0;
  saving = false;

  ngOnInit() {
    const productId = this.route.snapshot.paramMap.get('productId')!;
    this.catalogService.getProductById(productId).subscribe({
      next: (product) => {
        this.product = product;
        this.inventoryService.getByProductId(productId).subscribe({
          next: (inv) => {
            this.inventory = inv;
            this.loading = false;
            this.cdr.detectChanges();
          },
          error: () => {
            // no inventory record yet — show create form
            this.inventory = null;
            this.loading = false;
            this.cdr.detectChanges();
          }
        });
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  createInventory() {
    if (!this.product || this.createAmount <= 0) return;
    this.saving = true;
    this.inventoryService.create(this.product.id, this.product.name, this.createAmount).subscribe({
      next: (res) => {
        this.inventory = {
          id: res.id,
          productId: this.product!.id,
          quantity: this.createAmount,
          availableQuantity: this.createAmount,
          reservedQuantity: 0,
        };
        this.toast.show(`Added ${this.createAmount} units to inventory`);
        this.createAmount = 0;
        this.saving = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.toast.show('Failed to create inventory item', 'error');
        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }

  restock() {
    if (!this.inventory || this.restockAmount <= 0) return;
    this.saving = true;
    this.inventoryService.restock(this.inventory.id, this.restockAmount, this.inventory.quantity).subscribe({
      next: () => {
        this.inventory!.quantity += this.restockAmount;
        this.inventory!.availableQuantity += this.restockAmount;
        this.toast.show(`Restocked ${this.restockAmount} units successfully`);
        this.restockAmount = 0;
        this.saving = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.toast.show('Failed to restock', 'error');
        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }

  get statusLabel(): string {
    if (!this.inventory) return '';
    if (this.inventory.availableQuantity === 0) return 'Out of stock';
    if (this.inventory.availableQuantity <= 5) return 'Low stock';
    return 'In stock';
  }

  get statusClass(): string {
    if (!this.inventory) return '';
    if (this.inventory.availableQuantity === 0) return 'status-danger';
    if (this.inventory.availableQuantity <= 5) return 'status-warning';
    return 'status-success';
  }
}
