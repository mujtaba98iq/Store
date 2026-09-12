import { httpResource } from '@angular/common/http';
import { Injectable, Injector, inject } from '@angular/core';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { ErrorNotification, errorNotification } from '@app/core/utils/error-notification';
import { environment } from '@env/environment';
import { ProductOption } from '../models/product.model';

const PRODUCTS_URL = `${environment.apiBaseUrl}/products`;

/** High enough to hold the whole catalogue of a shop this size. */
const LOOKUP_PAGE_SIZE = 200;

/**
 * The products a variant can be opened against. Only the lookup is needed here -
 * products themselves are managed elsewhere, and this page never writes one.
 */
@Injectable({ providedIn: 'root' })
export class ApiProductsLookupService {
  private readonly injector = inject(Injector);

  /**
   * The whole catalogue, not a page of it - the toolbar filter and the form's
   * picker both choose one product out of the list. Sorted by name, so the two
   * menus read alphabetically rather than by whenever a product was added.
   *
   * A failed read is reported by whatever asked for it, which says so where the
   * picker would have been; a toast would only say the same thing twice.
   */
  list() {
    return httpResource<PaginatedResult<ProductOption>>(
      () => ({
        url: PRODUCTS_URL,
        // OrderBy 2 is Name, direction 0 is ascending.
        params: { Page: 1, PageSize: LOOKUP_PAGE_SIZE, OrderBy: 2, OrderByDirection: 0 },
        context: errorNotification(ErrorNotification.Silent),
      }),
      { injector: this.injector },
    );
  }
}
