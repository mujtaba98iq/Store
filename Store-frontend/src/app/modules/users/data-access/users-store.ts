import { Injectable, computed, inject, linkedSignal, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { OrderDirection } from '@app/core/models/enums/order-direction';
import { AuthStore } from '@app/core/services/common/auth-store';
import { ToastService } from '@app/core/services/common/toast';
import { describeError } from '@app/core/utils/api-error';
import { ApiUsersService } from '../api/users';
import { User, UserOrderBy, UserQuery, UserRole } from '../models/user.model';

const PAGE_SIZE = 10;
const SEARCH_DEBOUNCE_MS = 300;

export interface RoleOption {
  readonly label: string;
  /** Empty is the "everyone" pill: the filter is left off the request. */
  readonly value: string;
}

export const ROLE_FILTERS: readonly RoleOption[] = [
  { label: 'Everyone', value: '' },
  { label: 'Admins', value: UserRole.Admin },
  { label: 'Customers', value: UserRole.User },
];

export interface SortOption {
  readonly label: string;
  readonly value: string;
  readonly orderBy: UserOrderBy;
  readonly direction: OrderDirection;
}

export const SORT_OPTIONS: readonly SortOption[] = [
  {
    label: 'Newest first',
    value: 'newest',
    orderBy: UserOrderBy.CreatedAt,
    direction: OrderDirection.Desc,
  },
  {
    label: 'Oldest first',
    value: 'oldest',
    orderBy: UserOrderBy.CreatedAt,
    direction: OrderDirection.Asc,
  },
  {
    label: 'Username: A to Z',
    value: 'username-asc',
    orderBy: UserOrderBy.Username,
    direction: OrderDirection.Asc,
  },
  {
    label: 'Username: Z to A',
    value: 'username-desc',
    orderBy: UserOrderBy.Username,
    direction: OrderDirection.Desc,
  },
];

/**
 * The management page's state: the filters the admin has chosen, the page they are
 * on, and the delete they have in flight. Page-scoped, so a fresh visit starts
 * unfiltered.
 */
@Injectable()
export class UsersStore {
  private readonly api = inject(ApiUsersService);
  private readonly toasts = inject(ToastService);
  private readonly auth = inject(AuthStore);

  readonly roleFilters = ROLE_FILTERS;
  readonly sortOptions = SORT_OPTIONS;
  readonly pageSize = PAGE_SIZE;

  readonly searchInput = signal('');

  /** Debounced so typing does not fire a request per keystroke. */
  private readonly search = toSignal(
    toObservable(this.searchInput).pipe(debounceTime(SEARCH_DEBOUNCE_MS), distinctUntilChanged()),
    { initialValue: '' },
  );

  readonly role = signal('');
  readonly sortKey = signal(SORT_OPTIONS[0].value);

  /** Writable for the pager, but any filter change sends the admin back to page 1. */
  readonly page = linkedSignal<string, number>({
    source: () => `${this.search()}|${this.role()}|${this.sortKey()}`,
    computation: () => 1,
  });

  private readonly query = computed<UserQuery>(() => {
    const sort = SORT_OPTIONS.find((option) => option.value === this.sortKey()) ?? SORT_OPTIONS[0];

    return {
      page: this.page(),
      pageSize: PAGE_SIZE,
      username: this.search(),
      role: this.role(),
      orderBy: sort.orderBy,
      orderByDirection: sort.direction,
    };
  });

  private readonly users = this.api.search(this.query);

  private readonly deletingIdState = signal<string | null>(null);

  /** The account whose delete is in flight, so its row can show the progress. */
  readonly deletingId = this.deletingIdState.asReadonly();

  /**
   * The signed-in admin's own id. The API refuses to delete the account the caller
   * is signed in with, so their row does not offer the button in the first place.
   */
  readonly currentUserId = computed(() => this.auth.claims()?.userId ?? null);

  readonly isLoading = computed(() => this.users.isLoading());

  // Reading value() on a failed resource rethrows, so gate every read.
  readonly items = computed<readonly User[]>(() =>
    this.users.hasValue() ? (this.users.value()?.data ?? []) : [],
  );
  readonly totalCount = computed(() =>
    this.users.hasValue() ? (this.users.value()?.totalCount ?? 0) : 0,
  );
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE)));
  readonly errorMessage = computed(() => describeError(this.users.error()));

  readonly hasFilters = computed(() => this.searchInput().trim().length > 0 || this.role() !== '');

  setSearch(term: string): void {
    this.searchInput.set(term);
  }

  setRole(role: string): void {
    this.role.set(role);
  }

  setSort(key: string): void {
    this.sortKey.set(key);
  }

  clearFilters(): void {
    this.searchInput.set('');
    this.role.set('');
  }

  goToPage(page: number): void {
    this.page.set(Math.min(Math.max(1, page), this.totalPages()));
  }

  reload(): void {
    this.users.reload();
  }

  /**
   * Soft-deletes the account and refreshes the listing. A failure is reported by
   * `errorInterceptor` and leaves the row where it was.
   */
  remove(user: User): void {
    if (this.deletingIdState() !== null) {
      return;
    }

    this.deletingIdState.set(user.id);

    this.api.remove(user.id).subscribe({
      next: () => {
        this.deletingIdState.set(null);
        this.toasts.success('User deleted successfully.');
        this.afterRemoval();
      },
      error: () => this.deletingIdState.set(null),
    });
  }

  /**
   * Emptying the last page would otherwise strand the admin on a page that no
   * longer exists, so stepping back refetches; every other page just reloads.
   */
  private afterRemoval(): void {
    if (this.items().length === 1 && this.page() > 1) {
      this.page.set(this.page() - 1);
      return;
    }

    this.users.reload();
  }
}
