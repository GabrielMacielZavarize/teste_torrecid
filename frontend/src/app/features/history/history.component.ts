import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../../core/api/product.service';
import { StockService } from '../../core/api/stock.service';
import { errorMessage } from '../../core/error-message';
import { movementLabels } from '../../core/labels';
import { Product, StockMovement } from '../../core/models';

const pageSize = 20;

@Component({
  selector: 'app-history',
  imports: [DatePipe],
  templateUrl: './history.component.html',
})
export class HistoryComponent {
  private readonly stockService = inject(StockService);
  private readonly productService = inject(ProductService);

  protected readonly labels = movementLabels;
  protected readonly products = signal<Product[]>([]);
  protected readonly movements = signal<StockMovement[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly page = signal(1);
  protected readonly productId = signal(
    inject(ActivatedRoute).snapshot.queryParamMap.get('productId'),
  );
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / pageSize)),
  );

  constructor() {
    this.productService.list().subscribe({
      next: (products) => this.products.set(products),
      error: (error) => this.error.set(errorMessage(error)),
    });
    this.load();
  }

  protected filterByProduct(productId: string): void {
    this.productId.set(productId || null);
    this.page.set(1);
    this.load();
  }

  protected goToPage(page: number): void {
    this.page.set(page);
    this.load();
  }

  private load(): void {
    this.loading.set(true);

    this.stockService.history(this.productId(), this.page(), pageSize).subscribe({
      next: (result) => {
        this.movements.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(errorMessage(error));
        this.loading.set(false);
      },
    });
  }
}
