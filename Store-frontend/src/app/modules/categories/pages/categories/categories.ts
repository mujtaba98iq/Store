import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CategoryDetail } from '@app/core/models/interfaces/category';
import { Modal } from '@app/shared/components/modal/modal';
import { CategoryForm } from '../../components/category-form/category-form';
import { CategoriesStore, SearchField } from '../../data-access/categories-store';

@Component({
  selector: 'app-categories',
  imports: [DatePipe, Modal, CategoryForm],
  templateUrl: './categories.html',
  styleUrl: './categories.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [CategoriesStore],
})
export class Categories {
  private readonly store = inject(CategoriesStore);

  // The listing state lives in the store; the page only renders it.
  protected readonly searchFields = this.store.searchFields;
  protected readonly sortOptions = this.store.sortOptions;
  protected readonly searchInput = this.store.searchInput;
  protected readonly searchField = this.store.searchField;
  protected readonly sortKey = this.store.sortKey;
  protected readonly page = this.store.page;
  protected readonly pageSize = this.store.pageSize;
  protected readonly isLoading = this.store.isLoading;
  protected readonly items = this.store.items;
  protected readonly totalCount = this.store.totalCount;
  protected readonly totalPages = this.store.totalPages;
  protected readonly errorMessage = this.store.errorMessage;
  protected readonly deletingId = this.store.deletingId;
  protected readonly deleteError = this.store.deleteError;
  protected readonly hasFilters = this.store.hasFilters;

  protected readonly editing = signal<CategoryDetail | null>(null);
  protected readonly isFormOpen = signal(false);

  /** The category the admin has asked to delete, held until they confirm. */
  protected readonly pendingDelete = signal<CategoryDetail | null>(null);

  protected readonly lastSaved = signal('');

  /** Rows are numbered across pages, so the first column reads like a ledger. */
  protected rowNumber(index: number): number {
    return (this.page() - 1) * this.pageSize + index + 1;
  }

  protected onSearch(event: Event): void {
    this.store.setSearch((event.target as HTMLInputElement).value);
  }

  protected selectSearchField(field: SearchField): void {
    this.store.setSearchField(field);
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

  protected openEdit(category: CategoryDetail): void {
    this.editing.set(category);
    this.isFormOpen.set(true);
  }

  protected closeForm(): void {
    this.isFormOpen.set(false);
  }

  protected onSaved(category: CategoryDetail): void {
    this.isFormOpen.set(false);
    this.lastSaved.set(category.name);
    this.store.reload();
  }

  protected askToDelete(category: CategoryDetail): void {
    this.pendingDelete.set(category);
  }

  protected cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  protected confirmDelete(): void {
    const category = this.pendingDelete();
    if (!category) {
      return;
    }

    this.pendingDelete.set(null);
    this.store.remove(category);
  }
}
