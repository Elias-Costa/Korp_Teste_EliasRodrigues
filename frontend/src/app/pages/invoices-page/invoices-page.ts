import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { finalize, forkJoin, switchMap } from 'rxjs';

import { extractErrorMessage } from '../../core/config/extract-error-message';
import { CreateInvoiceRequest, Invoice } from '../../core/models/invoice';
import { Product } from '../../core/models/product';
import { InvoicesApiService } from '../../core/services/invoices-api.service';
import { ProductsApiService } from '../../core/services/products-api.service';

type FeedbackTone = 'success' | 'error';

interface FeedbackState {
  tone: FeedbackTone;
  title: string;
  message: string;
}

type InvoiceItemFormGroup = FormGroup<{
  productId: FormControl<string>;
  quantity: FormControl<number>;
}>;

@Component({
  selector: 'app-invoices-page',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './invoices-page.html',
  styleUrl: './invoices-page.scss',
})
export class InvoicesPage implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly invoicesApiService = inject(InvoicesApiService);
  private readonly productsApiService = inject(ProductsApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly invoiceForm = this.formBuilder.group({
    items: this.formBuilder.array<InvoiceItemFormGroup>([this.createItemGroup()]),
  });

  readonly products = signal<Product[]>([]);
  readonly invoices = signal<Invoice[]>([]);
  readonly feedback = signal<FeedbackState | null>(null);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isFailureModeEnabled = signal(false);
  readonly isFailureModeUpdating = signal(false);
  readonly printingInvoiceIds = signal<string[]>([]);
  readonly openInvoicesCount = computed(
    () => this.invoices().filter((invoice) => invoice.status === 'Open').length,
  );
  readonly printedInvoicesCount = computed(
    () => this.invoices().filter((invoice) => invoice.status === 'Closed').length,
  );
  readonly availableStock = computed(() =>
    this.products().reduce((sum, product) => sum + product.stock, 0),
  );

  get itemForms(): InvoiceItemFormGroup[] {
    return this.itemsArray.controls;
  }

  get itemsArray(): FormArray<InvoiceItemFormGroup> {
    return this.invoiceForm.controls.items;
  }

  ngOnInit(): void {
    this.loadPageData();
  }

  addItem(): void {
    this.itemsArray.push(this.createItemGroup());
  }

  removeItem(index: number): void {
    if (this.itemsArray.length === 1) {
      return;
    }

    this.itemsArray.removeAt(index);
  }

  submitInvoice(): void {
    if (this.invoiceForm.invalid) {
      this.invoiceForm.markAllAsTouched();
      return;
    }

    const payload: CreateInvoiceRequest = {
      items: this.itemsArray.getRawValue(),
    };

    this.isSaving.set(true);
    this.feedback.set(null);

    this.invoicesApiService
      .createInvoice(payload)
      .pipe(
        switchMap(() => this.invoicesApiService.getInvoices()),
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (invoices) => {
          this.invoices.set(invoices);
          this.resetInvoiceForm();
          this.feedback.set({
            tone: 'success',
            title: 'Nota criada',
            message: 'A nota foi salva com status Open e ja pode ser impressa.',
          });
        },
        error: (error: unknown) => {
          this.feedback.set({
            tone: 'error',
            title: 'Nao foi possivel criar a nota',
            message: extractErrorMessage(
              error,
              'Confira os produtos selecionados e tente novamente.',
            ),
          });
        },
      });
  }

  printInvoice(invoiceId: string): void {
    this.printingInvoiceIds.update((ids) => [...ids, invoiceId]);
    this.feedback.set(null);

    this.invoicesApiService
      .printInvoice(invoiceId)
      .pipe(
        switchMap(() =>
          forkJoin({
            invoices: this.invoicesApiService.getInvoices(),
            products: this.productsApiService.getProducts(),
          }),
        ),
        finalize(() =>
          this.printingInvoiceIds.update((ids) => ids.filter((currentId) => currentId !== invoiceId)),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: ({ invoices, products }) => {
          this.invoices.set(invoices);
          this.products.set(products);
          this.feedback.set({
            tone: 'success',
            title: 'Nota impressa',
            message: 'O status da nota foi atualizado para Closed e o estoque foi baixado.',
          });
        },
        error: (error: unknown) => {
          this.feedback.set({
            tone: 'error',
            title: 'Falha ao imprimir a nota',
            message: extractErrorMessage(
              error,
              'Ocorreu um problema ao processar a impressao da nota.',
            ),
          });
        },
      });
  }

  reloadData(): void {
    this.loadPageData();
  }

  toggleFailureMode(): void {
    const nextValue = !this.isFailureModeEnabled();
    this.isFailureModeUpdating.set(true);

    this.productsApiService
      .setFailureMode(nextValue)
      .pipe(finalize(() => this.isFailureModeUpdating.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.isFailureModeEnabled.set(response.enabled);
          this.feedback.set({
            tone: 'success',
            title: response.enabled ? 'Falha simulada ativada' : 'Falha simulada desativada',
            message: response.enabled
              ? 'Agora as impressoes vao falhar com feedback claro, mantendo a nota aberta.'
              : 'O fluxo normal de impressao foi restaurado.',
          });
        },
        error: (error: unknown) => {
          this.feedback.set({
            tone: 'error',
            title: 'Nao foi possivel atualizar a simulacao de falha',
            message: extractErrorMessage(error, 'Tente novamente em alguns segundos.'),
          });
        },
      });
  }

  isPrinting(invoiceId: string): boolean {
    return this.printingInvoiceIds().includes(invoiceId);
  }

  private loadPageData(): void {
    this.isLoading.set(true);

    forkJoin({
      products: this.productsApiService.getProducts(),
      invoices: this.invoicesApiService.getInvoices(),
      failureMode: this.productsApiService.getFailureMode(),
    })
      .pipe(finalize(() => this.isLoading.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ products, invoices, failureMode }) => {
          this.products.set(products);
          this.invoices.set(invoices);
          this.isFailureModeEnabled.set(failureMode.enabled);
        },
        error: (error: unknown) => {
          this.feedback.set({
            tone: 'error',
            title: 'Falha ao carregar a tela de notas',
            message: extractErrorMessage(
              error,
              'Verifique se os dois microsservicos estao rodando e tente novamente.',
            ),
          });
        },
      });
  }

  private resetInvoiceForm(): void {
    while (this.itemsArray.length > 1) {
      this.itemsArray.removeAt(this.itemsArray.length - 1);
    }

    this.itemsArray.at(0).reset({
      productId: '',
      quantity: 1,
    });

    this.invoiceForm.markAsPristine();
    this.invoiceForm.markAsUntouched();
  }

  private createItemGroup(): InvoiceItemFormGroup {
    return this.formBuilder.nonNullable.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
    });
  }
}
