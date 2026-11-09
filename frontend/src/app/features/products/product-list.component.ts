import { Component, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../core/api/product.service';
import { AuthService } from '../../core/auth/auth.service';
import { errorMessage } from '../../core/error-message';
import { Product } from '../../core/models';

@Component({
  selector: 'app-product-list',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './product-list.component.html',
})
export class ProductListComponent {
  private readonly productService = inject(ProductService);
  private readonly auth = inject(AuthService);

  protected readonly canManage = computed(() => this.auth.hasPermission('Products.Manage'));
  protected readonly canMove = computed(() => this.auth.hasPermission('Stock.Move'));

  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly search = signal('');
  protected readonly filteredProducts = computed(() => {
    const term = this.search().trim().toLowerCase();
    return this.products().filter(
      (product) =>
        product.name.toLowerCase().includes(term) || product.sku.toLowerCase().includes(term),
    );
  });

  protected readonly editing = signal<Product | 'new' | null>(null);
  protected readonly submitted = signal(false);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly confirmingDeleteId = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    sku: ['', [Validators.required, Validators.maxLength(50)]],
    name: ['', [Validators.required, Validators.maxLength(150)]],
    description: ['', Validators.maxLength(500)],
  });

  constructor() {
    this.load();
  }

  protected openCreate(): void {
    this.prepareForm();
    this.form.controls.sku.enable();
    this.editing.set('new');
  }

  protected openEdit(product: Product): void {
    this.prepareForm();
    this.form.patchValue({
      sku: product.sku,
      name: product.name,
      description: product.description ?? '',
    });
    this.form.controls.sku.disable();
    this.editing.set(product);
  }

  protected closeForm(): void {
    this.editing.set(null);
  }

  protected save(): void {
    this.submitted.set(true);
    const editing = this.editing();
    if (this.form.invalid || this.saving() || editing === null) {
      return;
    }

    const { sku, name, description } = this.form.getRawValue();
    const input = { name, description: description.trim() || null };
    const request =
      editing === 'new'
        ? this.productService.create({ ...input, sku })
        : this.productService.update(editing.id, input);

    this.saving.set(true);
    this.formError.set(null);

    request.subscribe({
      next: (product) => {
        this.saving.set(false);
        this.editing.set(null);
        this.success.set(`Produto ${product.sku} salvo.`);
        this.load();
      },
      error: (error) => {
        this.saving.set(false);
        this.formError.set(errorMessage(error));
      },
    });
  }

  protected confirmDelete(product: Product): void {
    this.error.set(null);
    this.success.set(null);

    this.productService.delete(product.id).subscribe({
      next: () => {
        this.confirmingDeleteId.set(null);
        this.success.set(`Produto ${product.sku} excluído.`);
        this.load();
      },
      error: (error) => {
        this.confirmingDeleteId.set(null);
        this.error.set(errorMessage(error));
      },
    });
  }

  private prepareForm(): void {
    this.form.reset();
    this.submitted.set(false);
    this.formError.set(null);
    this.success.set(null);
  }

  private load(): void {
    this.productService.list().subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(errorMessage(error));
        this.loading.set(false);
      },
    });
  }
}
