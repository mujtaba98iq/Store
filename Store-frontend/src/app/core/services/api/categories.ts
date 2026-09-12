import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, Injector, Signal, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import {
  Category,
  CategoryDetail,
  CategoryQuery,
  CreateCategoryBody,
  UpdateCategoryBody,
} from '@app/core/models/interfaces/category';
import { environment } from '@env/environment';

const CATEGORIES_URL = `${environment.apiBaseUrl}/categories`;

/** High enough to hold every category a shop of this size has. */
const LOOKUP_PAGE_SIZE = 100;

function toParams(query: CategoryQuery): Record<string, string | number> {
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

  const description = query.description.trim();
  if (description) {
    params['Description'] = description;
  }

  return params;
}

/**
 * One method per endpoint of the API's categories resource. Categories are both a
 * lookup the catalogue filters by and a resource the admin manages, so this client
 * is shared: the catalogue reads `list()`, the management page uses the rest.
 */
@Injectable({ providedIn: 'root' })
export class ApiCategoriesService {
  private readonly http = inject(HttpClient);
  private readonly injector = inject(Injector);

  /** Every category, not a page of them - the filter bar needs the full list. */
  list() {
    return httpResource<PaginatedResult<Category>>(
      () => ({ url: CATEGORIES_URL, params: { Page: 1, PageSize: LOOKUP_PAGE_SIZE } }),
      { injector: this.injector },
    );
  }

  /** Reactive management listing - refetches whenever `query` changes. */
  search(query: Signal<CategoryQuery>) {
    return httpResource<PaginatedResult<CategoryDetail>>(
      () => ({ url: CATEGORIES_URL, params: toParams(query()) }),
      { injector: this.injector },
    );
  }

  create(body: CreateCategoryBody): Observable<CategoryDetail> {
    return this.http.post<CategoryDetail>(CATEGORIES_URL, body);
  }

  update(id: string, body: UpdateCategoryBody): Observable<CategoryDetail> {
    return this.http.patch<CategoryDetail>(`${CATEGORIES_URL}/${id}`, body);
  }

  /** Soft-deletes the category; the API answers 204 with no body. */
  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${CATEGORIES_URL}/${id}`);
  }
}
