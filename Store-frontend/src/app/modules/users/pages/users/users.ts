import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Modal } from '@app/shared/components/modal/modal';
import { Pager } from '@app/shared/components/pager/pager';
import { UserForm } from '../../components/user-form/user-form';
import { UsersStore } from '../../data-access/users-store';
import { User } from '../../models/user.model';

@Component({
  selector: 'app-users',
  imports: [DatePipe, Modal, Pager, UserForm],
  templateUrl: './users.html',
  styleUrl: './users.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [UsersStore],
})
export class Users {
  private readonly store = inject(UsersStore);

  // The listing state lives in the store; the page only renders it.
  protected readonly roleFilters = this.store.roleFilters;
  protected readonly sortOptions = this.store.sortOptions;
  protected readonly searchInput = this.store.searchInput;
  protected readonly role = this.store.role;
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
  protected readonly currentUserId = this.store.currentUserId;

  protected readonly editing = signal<User | null>(null);
  protected readonly isFormOpen = signal(false);

  /** The account the admin has asked to delete, held until they confirm. */
  protected readonly pendingDelete = signal<User | null>(null);

  /** Rows are numbered across pages, so the first column reads like a ledger. */
  protected rowNumber(index: number): number {
    return (this.page() - 1) * this.pageSize + index + 1;
  }

  /**
   * The API refuses to delete the account the caller is signed in with, so that row
   * is marked instead of offering a button that could only fail.
   */
  protected isSelf(user: User): boolean {
    return user.id.toLowerCase() === (this.currentUserId() ?? '').toLowerCase();
  }

  protected onSearch(event: Event): void {
    this.store.setSearch((event.target as HTMLInputElement).value);
  }

  protected selectRole(value: string): void {
    this.store.setRole(value);
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

  protected openEdit(user: User): void {
    this.editing.set(user);
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

  protected askToDelete(user: User): void {
    this.pendingDelete.set(user);
  }

  protected cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  protected confirmDelete(): void {
    const user = this.pendingDelete();
    if (!user) {
      return;
    }

    this.pendingDelete.set(null);
    this.store.remove(user);
  }
}
