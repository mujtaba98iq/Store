import { httpResource } from '@angular/common/http';
import { Category, PaginatedResult } from '../products/product';

const CATEGORIES_URL = '/api/categories';

/** The filter bar needs every category, not a page of them. */
export function categoriesResource() {
  return httpResource<PaginatedResult<Category>>(() => ({
    url: CATEGORIES_URL,
    params: { Page: 1, PageSize: 100 },
  }));
}
