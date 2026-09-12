import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Modal } from '@app/shared/components/modal/modal';
import { InventoryForm } from '../../components/inventory-form/inventory-form';
import { InventoriesStore } from '../../data-access/inventories-store';
import { Inventory } from '../../models/inventory.model';

/** Below this a row is called out as running low rather than just being in stock. */
const LOW_STOCK_THRESHOLD = 5;

@Component({
  selector: 'app-inventories',
  imports: [DatePipe, Modal, InventoryForm],
  templateUrl: './inventories.html',
  styleUrl: './inventories.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [InventoriesStore],
})
export class Inventories {
  private readonly store = inject(InventoriesStore);

  // The listing state lives in the store; the page only renders it.
  protected readonly availabilityFilters = this.store.availabilityFilters;
  protected readonly sortOptions = this.store.sortOptions;
  protected readonly searchInput = this.store.searchInput;
  protected readonly minInput = this.store.minInput;
  protected readonly maxInput = this.store.maxInput;
  protected readonly availability = this.store.availability;
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
  protected readonly variantOptions = this.store.variantOptions;
  protected readonly variantsLoading = this.store.variantsLoading;

  protected readonly editing = signal<Inventory | null>(null);
  protected readonly isFormOpen = signal(false);

  /** The row the admin has asked to delete, held until they confirm. */
  protected readonly pendingDelete = signal<Inventory | null>(null);

  /** Rows are numbered across pages, so the first column reads like a ledger. */
  protected rowNumber(index: number): number {
    return (this.page() - 1) * this.pageSize + index + 1;
  }

  /** A row with nothing left to sell; the shop cannot fill an order from it. */
  protected isSoldOut(inventory: Inventory): boolean {
    return inventory.availableQuantity <= 0;
  }

  /** In stock, but close enough to empty to be worth topping up. */
  protected isLow(inventory: Inventory): boolean {
    return inventory.availableQuantity > 0 && inventory.availableQuantity < LOW_STOCK_THRESHOLD;
  }

  /** The variant behind a row can be gone; the SKU is all the page has to show. */
  protected labelFor(inventory: Inventory): string {
    return inventory.sku ?? 'Variant removed';
  }

  protected onSearch(event: Event): void {
    this.store.setSearch((event.target as HTMLInputElement).value);
  }

  protected onMinQuantity(event: Event): void {
    this.store.setMinQuantity((event.target as HTMLInputElement).value);
  }

  protected onMaxQuantity(event: Event): void {
    this.store.setMaxQuantity((event.target as HTMLInputElement).value);
  }

  protected selectAvailability(value: string): void {
    this.store.setAvailability(value);
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

  protected openEdit(inventory: Inventory): void {
    this.editing.set(inventory);
    this.isFormOpen.set(true);
  }

  protected closeForm(): void {
    this.isFormOpen.set(false);
  }

  // The store announced the save; the page only has to close up and refetch.
  protected onSaved(): void {
    this.isFormOpen.set(false);
    this.store.reloadAll();
  }

  protected askToDelete(inventory: Inventory): void {
    this.pendingDelete.set(inventory);
  }

  protected cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  protected confirmDelete(): void {
    const inventory = this.pendingDelete();
    if (!inventory) {
      return;
    }

    this.pendingDelete.set(null);
    this.store.remove(inventory);
  }
}
