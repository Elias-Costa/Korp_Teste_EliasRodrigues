import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, Observable, switchMap } from 'rxjs';

import { extractErrorMessage } from '../../core/config/extract-error-message';
import { Product } from '../../core/models/product';
import { ProductsApiService } from '../../core/services/products-api.service';

type FeedbackTone = 'success' | 'error';

interface FeedbackState {
  tone: FeedbackTone;
  title: string;
  message: string;
}

@Component({
  selector: 'app-products-page',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './products-page.html',
  styleUrl: './products-page.scss',
})
export class ProductsPage implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly productsApiService = inject(ProductsApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly productForm = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    description: ['', [Validators.required, Validators.maxLength(200)]],
    stock: [0, [Validators.required, Validators.min(0)]],
  });

  readonly products = signal<Product[]>([]);
  readonly feedback = signal<FeedbackState | null>(null);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly editingProduct = signal<Product | null>(null);
  readonly isEditing = computed(() => this.editingProduct() !== null);
  readonly totalStock = computed(() =>
    this.products().reduce((sum, product) => sum + product.stock, 0),
  );

  ngOnInit(): void {
    this.loadProducts();
  }

  submitProduct(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.feedback.set(null);

    const currentProduct = this.editingProduct();
    const request$ = this.getSaveRequest(currentProduct);

    request$
      .pipe(
        switchMap(() => this.productsApiService.getProducts()),
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (products) => {
          this.products.set(products);
          this.resetForm();
          this.feedback.set({
            tone: 'success',
            title: currentProduct ? 'Produto atualizado' : 'Produto cadastrado',
            message: currentProduct
              ? 'As informacoes do produto foram atualizadas com sucesso.'
              : 'O produto foi salvo e ja pode ser usado na criacao de notas.',
          });
        },
        error: (error: unknown) => {
          this.feedback.set({
            tone: 'error',
            title: currentProduct
              ? 'Nao foi possivel atualizar o produto'
              : 'Nao foi possivel cadastrar o produto',
            message: extractErrorMessage(error, 'Confira os campos e tente novamente.'),
          });
        },
      });
  }

  startEditing(product: Product): void {
    this.editingProduct.set(product);
    this.feedback.set(null);
    this.productForm.setValue({
      code: product.code,
      description: product.description,
      stock: product.stock,
    });
    this.productForm.markAsPristine();
    this.productForm.markAsUntouched();
  }

  cancelEditing(): void {
    this.resetForm();
    this.feedback.set({
      tone: 'success',
      title: 'Edicao cancelada',
      message: 'O formulario voltou para o modo de cadastro.',
    });
  }

  reloadProducts(): void {
    this.loadProducts();
  }

  private loadProducts(): void {
    this.isLoading.set(true);

    this.productsApiService
      .getProducts()
      .pipe(finalize(() => this.isLoading.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (products) => {
          this.products.set(products);
          const currentProduct = this.editingProduct();

          if (currentProduct) {
            const updatedProduct = products.find((product) => product.id === currentProduct.id) ?? null;
            this.editingProduct.set(updatedProduct);
          }
        },
        error: (error: unknown) => {
          this.feedback.set({
            tone: 'error',
            title: 'Falha ao carregar produtos',
            message: extractErrorMessage(
              error,
              'Verifique se o inventory-service esta rodando e tente novamente.',
            ),
          });
        },
      });
  }

  private resetForm(): void {
    this.editingProduct.set(null);
    this.productForm.reset({
      code: '',
      description: '',
      stock: 0,
    });
    this.productForm.markAsPristine();
    this.productForm.markAsUntouched();
  }

  private getSaveRequest(currentProduct: Product | null): Observable<Product> {
    const payload = this.productForm.getRawValue();

    return currentProduct
      ? this.productsApiService.updateProduct(currentProduct.id, payload)
      : this.productsApiService.createProduct(payload);
  }
}
