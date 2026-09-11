import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { AuthStore } from '@app/core/services/common/auth-store';
import { Modal } from '@app/shared/components/modal/modal';
import { ProductCard } from '@app/shared/components/product-card/product-card';
import { ProductForm } from '../../components/product-form/product-form';
import { ProductsStore } from '../../data-access/products-store';
import { Product } from '../../models/product.model';
import { productImageUrl } from '../../utils/product-image';

@Component({
  selector: 'app-products',
  imports: [Modal, ProductCard, ProductForm],
  templateUrl: './products.html',
  styleUrl: './products.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ProductsStore],
})
export class Products {
  private readonly auth = inject(AuthStore);
  private readonly store = inject(ProductsStore);

  protected readonly isAdmin = this.auth.isAdmin;

  // The catalogue state lives in the store; the page only renders it.
  protected readonly sortOptions = this.store.sortOptions;
  protected readonly searchInput = this.store.searchInput;
  protected readonly activeCategoryId = this.store.activeCategoryId;
  protected readonly sortKey = this.store.sortKey;
  protected readonly page = this.store.page;
  protected readonly isLoading = this.store.isLoading;
  protected readonly items = this.store.items;
  protected readonly totalCount = this.store.totalCount;
  protected readonly totalPages = this.store.totalPages;
  protected readonly errorMessage = this.store.errorMessage;
  protected readonly categoryList = this.store.categoryList;
  protected readonly filters = this.store.filters;

  /** Uploaded image first, then the legacy imagePath. */
  protected readonly imageFor = productImageUrl;

  protected readonly editing = signal<Product | null>(null);
  protected readonly isFormOpen = signal(false);
  protected readonly lastAdded = signal('');

  protected selectCategory(categoryId: string | null): void {
    this.store.selectCategory(categoryId);
  }

  protected onSearch(event: Event): void {
    this.store.setSearch((event.target as HTMLInputElement).value);
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

  protected openEdit(product: Product): void {
    this.editing.set(product);
    this.isFormOpen.set(true);
  }

  protected closeForm(): void {
    this.isFormOpen.set(false);
  }

  protected onSaved(): void {
    this.isFormOpen.set(false);
    this.store.reload();
  }

  // No cart service yet - announce the action so the button is not a dead end.
  protected addToBag(product: Product): void {
    this.lastAdded.set(product.name);
  }
}
