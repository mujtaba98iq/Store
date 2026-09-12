import { httpResource } from '@angular/common/http';
import { Injectable, Injector, inject } from '@angular/core';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { ErrorNotification, errorNotification } from '@app/core/utils/error-notification';
import { environment } from '@env/environment';
import { ProductVariant } from '../models/product-variant.model';

const PRODUCT_VARIANTS_URL = `${environment.apiBaseUrl}/productvariants`;

/** High enough to hold every variant a shop of this size has. */
const LOOKUP_PAGE_SIZE = 200;

/**
 * The variants a stock row can be opened against. Only the lookup is needed here -
 * variants themselves are managed elsewhere, and this page never writes one.
 */
@Injectable({ providedIn: 'root' })
export class ApiProductVariantsService {
  private readonly injector = inject(Injector);

  /**
   * Every variant, not a page of them - the stock form picks one out of the list.
   * A failed read is reported by the form that asked for it, which says so where the
   * picker would have been; a toast would only say the same thing twice.
   */
  list() {
    return httpResource<PaginatedResult<ProductVariant>>(
      () => ({
        url: PRODUCT_VARIANTS_URL,
        params: { Page: 1, PageSize: LOOKUP_PAGE_SIZE },
        context: errorNotification(ErrorNotification.Silent),
      }),
      { injector: this.injector },
    );
  }
}
