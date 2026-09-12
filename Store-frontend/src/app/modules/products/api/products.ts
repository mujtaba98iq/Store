import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, Injector, Signal, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { ErrorNotification, errorNotification } from '@app/core/utils/error-notification';
import { environment } from '@env/environment';
import {
  CreateProductBody,
  Product,
  ProductQuery,
  UpdateProductBody,
} from '../models/product.model';

const PRODUCTS_URL = `${environment.apiBaseUrl}/products`;

function toParams(query: ProductQuery): Record<string, string | number> {
  const params: Record<string, string | number> = {
    Page: query.page,
    PageSize: query.pageSize,
    OrderBy: query.orderBy,
    OrderByDirection: query.orderByDirection,
  };

  // Omitted rather than sent empty, so the API skips the filter entirely.
  const name = query.name.trim();
  if (name) {
    params['Name'] = name;
  }
  if (query.categoryId) {
    params['CategoryId'] = query.categoryId;
  }

  return params;
}

/** One method per endpoint of the API's products resource. */
@Injectable({ providedIn: 'root' })
export class ApiProductsService {
  private readonly http = inject(HttpClient);
  private readonly injector = inject(Injector);

  /**
   * Reactive catalogue listing - refetches whenever `query` changes. A failed read
   * is reported by the page that asked for it, which can offer a retry; a toast
   * would only say the same thing twice.
   */
  list(query: Signal<ProductQuery>) {
    return httpResource<PaginatedResult<Product>>(
      () => ({
        url: PRODUCTS_URL,
        params: toParams(query()),
        context: errorNotification(ErrorNotification.Silent),
      }),
      { injector: this.injector },
    );
  }

  create(body: CreateProductBody): Observable<Product> {
    return this.http.post<Product>(PRODUCTS_URL, body, {
      context: errorNotification(ErrorNotification.FieldsInline),
    });
  }

  /**
   * `notify` is open so the caller can silence a write it has already decided not
   * to act on - see the last call of `ProductFormStore.createWithImage`.
   */
  update(
    id: string,
    body: UpdateProductBody,
    notify: ErrorNotification = ErrorNotification.FieldsInline,
  ): Observable<Product> {
    return this.http.patch<Product>(`${PRODUCTS_URL}/${id}`, body, {
      context: errorNotification(notify),
    });
  }
}
