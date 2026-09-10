import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateProductBody,
  PaginatedResult,
  Product,
  ProductQuery,
  UpdateProductBody,
} from './product';

const PRODUCTS_URL = '/api/products';

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

/** Reactive catalogue listing - refetches whenever `query` changes. */
export function productsResource(query: Signal<ProductQuery>) {
  return httpResource<PaginatedResult<Product>>(() => ({
    url: PRODUCTS_URL,
    params: toParams(query()),
  }));
}

@Injectable({ providedIn: 'root' })
export class ProductsApi {
  private readonly http = inject(HttpClient);

  create(body: CreateProductBody): Observable<Product> {
    return this.http.post<Product>(PRODUCTS_URL, body);
  }

  update(id: string, body: UpdateProductBody): Observable<Product> {
    return this.http.patch<Product>(`${PRODUCTS_URL}/${id}`, body);
  }
}
