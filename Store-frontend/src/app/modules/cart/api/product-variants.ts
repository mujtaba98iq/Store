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
 * The variants the cart's lines point at. Only the lookup is needed here - the cart
 * never writes a variant.
 */
@Injectable({ providedIn: 'root' })
export class ApiCartVariantsLookupService {
  private readonly injector = inject(Injector);

  /**
   * Every variant, not a page of them, and inactive ones included: a variant taken
   * off sale after it was carted still has to be named in the row that holds it.
   *
   * A failed read is not worth a toast - the cart itself still adds up, and the
   * rows fall back to their ids rather than going blank.
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
