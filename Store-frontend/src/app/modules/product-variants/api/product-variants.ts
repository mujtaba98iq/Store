import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, Injector, Signal, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { ErrorNotification, errorNotification } from '@app/core/utils/error-notification';
import { environment } from '@env/environment';
import {
  CreateProductVariantBody,
  ProductVariant,
  ProductVariantQuery,
  UpdateProductVariantBody,
} from '../models/product-variant.model';

const PRODUCT_VARIANTS_URL = `${environment.apiBaseUrl}/productvariants`;

function toParams(query: ProductVariantQuery): Record<string, string | number | boolean> {
  const params: Record<string, string | number | boolean> = {
    Page: query.page,
    PageSize: query.pageSize,
    OrderBy: query.orderBy,
    OrderByDirection: query.orderByDirection,
  };

  // Omitted rather than sent empty, so the API skips the filter entirely.
  const sku = query.sku.trim();
  if (sku) {
    params['Sku'] = sku;
  }

  const barcode = query.barcode.trim();
  if (barcode) {
    params['Barcode'] = barcode;
  }

  if (query.productId) {
    params['ProductId'] = query.productId;
  }

  // False is a filter of its own - "inactive only" - so only null is left off.
  if (query.isActive !== null) {
    params['IsActive'] = query.isActive;
  }

  return params;
}

/**
 * One method per endpoint of the API's product variants resource. Reads are open to
 * any signed-in account, but everything that writes is behind the Admin role, so
 * this client is only ever reached from the management page.
 */
@Injectable({ providedIn: 'root' })
export class ApiProductVariantsService {
  private readonly http = inject(HttpClient);
  private readonly injector = inject(Injector);

  /**
   * Reactive management listing - refetches whenever `query` changes. A failed read
   * is reported by the page that asked for it, which can offer a retry; a toast
   * would only say the same thing twice.
   */
  search(query: Signal<ProductVariantQuery>) {
    return httpResource<PaginatedResult<ProductVariant>>(
      () => ({
        url: PRODUCT_VARIANTS_URL,
        params: toParams(query()),
        context: errorNotification(ErrorNotification.Silent),
      }),
      { injector: this.injector },
    );
  }

  create(body: CreateProductVariantBody): Observable<ProductVariant> {
    return this.http.post<ProductVariant>(PRODUCT_VARIANTS_URL, body, {
      context: errorNotification(ErrorNotification.FieldsInline),
    });
  }

  update(id: string, body: UpdateProductVariantBody): Observable<ProductVariant> {
    return this.http.patch<ProductVariant>(`${PRODUCT_VARIANTS_URL}/${id}`, body, {
      context: errorNotification(ErrorNotification.FieldsInline),
    });
  }

  /** Soft-deletes the variant; the API answers 204 with no body. */
  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${PRODUCT_VARIANTS_URL}/${id}`);
  }
}
