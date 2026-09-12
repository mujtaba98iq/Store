import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Modal } from '@app/shared/components/modal/modal';
import { Pager } from '@app/shared/components/pager/pager';
import { ProductVariantForm } from '../../components/product-variant-form/product-variant-form';
import { ProductVariantsStore } from '../../data-access/product-variants-store';
import { ProductVariant } from '../../models/product-variant.model';

@Component({
  selector: 'app-product-variants',
  imports: [CurrencyPipe, DatePipe, Modal, Pager, ProductVariantForm],
  templateUrl: './product-variants.html',
  styleUrl: './product-variants.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ProductVariantsStore],
})
export class ProductVariants {
  private readonly store = inject(ProductVariantsStore);

  // The listing state lives in the store; the page only renders it.
  protected readonly statusFilters = this.store.statusFilters;
  protected readonly sortOptions = this.store.sortOptions;
  protected readonly searchInput = this.store.searchInput;
  protected readonly barcodeInput = this.store.barcodeInput;
  protected readonly productId = this.store.productId;
  protected readonly status = this.store.status;
  protected readonly sortKey = this.store.sortKey;
  protected readonly page = this.store.page;
  protected readonly pageSize = this.store.pageSize;
  protected readonly isLoading = this.store.isLoading;
  protected readonly items = this.store.items;
  protected readonly totalCount = this.store.totalCount;
  protected readonly totalPages = this.store.totalPages;
  protected readonly errorMessage = this.store.errorMessage;
  protected readonly deletingId = this.store.deletingId;
  protected readonly hasFilters = this.store.hasFilters;
  protected readonly productOptions = this.store.productOptions;
  protected readonly productsLoading = this.store.productsLoading;

  protected readonly editing = signal<ProductVariant | null>(null);
  protected readonly isFormOpen = signal(false);

  /** The row the admin has asked to delete, held until they confirm. */
  protected readonly pendingDelete = signal<ProductVariant | null>(null);

  /** Rows are numbered across pages, so the first column reads like a ledger. */
  protected rowNumber(index: number): number {
    return (this.page() - 1) * this.pageSize + index + 1;
  }

  /** The product behind a variant can be gone; there is nothing else to show. */
  protected productLabel(variant: ProductVariant): string {
    return variant.productName ?? 'Product removed';
  }

  protected onSearch(event: Event): void {
    this.store.setSearch((event.target as HTMLInputElement).value);
  }

  protected onBarcode(event: Event): void {
    this.store.setBarcode((event.target as HTMLInputElement).value);
  }

  protected onProductChange(event: Event): void {
    this.store.setProduct((event.target as HTMLSelectElement).value);
  }

  protected selectStatus(value: string): void {
    this.store.setStatus(value);
  }

  protected onSortChange(event: Event): void {
    this.store.setSort((event.target as HTMLSelectElement).value);
  }

  protected clearFilters(): void {
    this.store.clearFilters();
  }

  protected goToPage(page: number): void {
    this.store.goToPage(page);
  }

  protected retry(): void {
    this.store.reload();
  }

  protected openCreate(): void {
    this.editing.set(null);
    this.isFormOpen.set(true);
  }

  protected openEdit(variant: ProductVariant): void {
    this.editing.set(variant);
    this.isFormOpen.set(true);
  }

  protected closeForm(): void {
    this.isFormOpen.set(false);
  }

  // The store announced the save; the page only has to close up and refetch.
  protected onSaved(): void {
    this.isFormOpen.set(false);
    this.store.reload();
  }

  protected askToDelete(variant: ProductVariant): void {
    this.pendingDelete.set(variant);
  }

  protected cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  protected confirmDelete(): void {
    const variant = this.pendingDelete();
    if (!variant) {
      return;
    }

    this.pendingDelete.set(null);
    this.store.remove(variant);
  }
}
