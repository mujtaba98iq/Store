import { CategoryOrderBy } from '@app/core/models/enums/category-order-by';
import { OrderDirection } from '@app/core/models/enums/order-direction';

/**
 * The part of `RestApi.Categories.CategoryResponse` every reader needs: enough to
 * label a product or fill a filter pill.
 */
export interface Category {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
}

/** The whole response, audit columns included - what the management page lists. */
export interface CategoryDetail extends Category {
  readonly createdAt: string;
  readonly updatedAt: string | null;
  readonly createdById: string;
  readonly updatedById: string | null;
}

/**
 * Mirrors the query string bound to `Domain.Categories.CategoryFilters`.
 *
 * `name` and `description` are two separate `LIKE` filters that the API ANDs
 * together, so the page sends one of them at a time.
 */
export interface CategoryQuery {
  readonly page: number;
  readonly pageSize: number;
  readonly name: string;
  readonly description: string;
  readonly orderBy: CategoryOrderBy;
  readonly orderByDirection: OrderDirection;
}

/** Mirrors `RestApi.Categories.CreateCategoryRequest`. */
export interface CreateCategoryBody {
  readonly name: string;
  readonly description: string | null;
}

/** Mirrors `RestApi.Categories.UpdateCategoryRequest` - every field is optional. */
export type UpdateCategoryBody = Partial<CreateCategoryBody>;
