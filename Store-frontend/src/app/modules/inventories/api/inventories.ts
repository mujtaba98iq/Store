import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, Injector, Signal, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { ErrorNotification, errorNotification } from '@app/core/utils/error-notification';
import { environment } from '@env/environment';
import {
  CreateInventoryBody,
  Inventory,
  InventoryQuery,
  UpdateInventoryBody,
} from '../models/inventory.model';

const INVENTORIES_URL = `${environment.apiBaseUrl}/inventories`;

function toParams(query: InventoryQuery): Record<string, string | number | boolean> {
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

  // False is a filter of its own - "sold out only" - so only null is left off.
  if (query.isAvailable !== null) {
    params['IsAvailable'] = query.isAvailable;
  }

  // Zero is a meaningful bound, so these are checked against null rather than
  // for truthiness.
  if (query.minQuantity !== null) {
    params['MinQuantity'] = query.minQuantity;
  }

  if (query.maxQuantity !== null) {
    params['MaxQuantity'] = query.maxQuantity;
  }

  return params;
}

/**
 * One method per endpoint of the API's inventories resource. Reads are open to any
 * signed-in account, but everything that writes is behind the Admin role, so this
 * client is only ever reached from the management page.
 */
@Injectable({ providedIn: 'root' })
export class ApiInventoriesService {
  private readonly http = inject(HttpClient);
  private readonly injector = inject(Injector);

  /**
   * Reactive management listing - refetches whenever `query` changes. A failed read
   * is reported by the page that asked for it, which can offer a retry; a toast
   * would only say the same thing twice.
   */
  search(query: Signal<InventoryQuery>) {
    return httpResource<PaginatedResult<Inventory>>(
      () => ({
        url: INVENTORIES_URL,
        params: toParams(query()),
        context: errorNotification(ErrorNotification.Silent),
      }),
      { injector: this.injector },
    );
  }

  create(body: CreateInventoryBody): Observable<Inventory> {
    return this.http.post<Inventory>(INVENTORIES_URL, body, {
      context: errorNotification(ErrorNotification.FieldsInline),
    });
  }

  update(id: string, body: UpdateInventoryBody): Observable<Inventory> {
    return this.http.patch<Inventory>(`${INVENTORIES_URL}/${id}`, body, {
      context: errorNotification(ErrorNotification.FieldsInline),
    });
  }

  /** Soft-deletes the stock row; the API answers 204 with no body. */
  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${INVENTORIES_URL}/${id}`);
  }
}
