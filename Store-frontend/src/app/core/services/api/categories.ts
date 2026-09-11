import { httpResource } from '@angular/common/http';
import { Injectable, Injector, inject } from '@angular/core';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { Category } from '@app/core/models/interfaces/category';
import { environment } from '@env/environment';

const CATEGORIES_URL = `${environment.apiBaseUrl}/categories`;

/**
 * Categories are a lookup rather than a feature of their own: the catalogue
 * filters by them and the product form assigns them.
 */
@Injectable({ providedIn: 'root' })
export class ApiCategoriesService {
  private readonly injector = inject(Injector);

  /** Every category, not a page of them - the filter bar needs the full list. */
  list() {
    return httpResource<PaginatedResult<Category>>(
      () => ({ url: CATEGORIES_URL, params: { Page: 1, PageSize: 100 } }),
      { injector: this.injector },
    );
  }
}
