/** Mirrors `RestApi.Categories.CategoryResponse`. */
export interface Category {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
}

/** Mirrors `RestApi.ProductImages.ProductImageResponse`. */
export interface ProductImage {
  readonly id: string;
  readonly productId: string;
  readonly imageUrl: string;
  readonly isPrimary: boolean;
  readonly displayOrder: number;
}

/** Mirrors `RestApi.Products.ProductResponse`. */
export interface Product {
  readonly id: string;
  readonly name: string;
  readonly description: string;
  readonly price: number | null;
  readonly quantity: number | null;
  readonly imagePath: string;
  readonly createdAt: string;
  readonly updatedAt: string | null;
  readonly categories: readonly Category[];
  readonly images: readonly ProductImage[];
}

/** Only an absolute http(s) URL can be fetched; anything else would 404. */
function isFetchable(url: string | null | undefined): boolean {
  return /^https?:\/\//i.test((url ?? '').trim());
}

/**
 * The image to render: the uploaded primary image, then the remaining uploads by
 * display order, then the legacy `imagePath` column. Candidates that are not
 * absolute URLs are skipped rather than requested - relative paths point at files
 * the API no longer serves, so fetching them only produces a 404. Returns '' when
 * nothing is usable, and the card draws a placeholder instead.
 */
export function productImageUrl(product: Product): string {
  const uploaded = [...product.images]
    .sort((a, b) => Number(b.isPrimary) - Number(a.isPrimary) || a.displayOrder - b.displayOrder)
    .map((image) => image.imageUrl);

  return [...uploaded, product.imagePath].find(isFetchable)?.trim() ?? '';
}

/** Mirrors `Sheard.Type.PaginationResult<T>`. */
export interface PaginatedResult<T> {
  readonly totalCount: number;
  readonly data: readonly T[];
}

/** Mirrors `Domain.Products.ProductOrderBy`. */
export const ProductOrderBy = {
  CreatedAt: 1,
  Name: 2,
  Price: 3,
} as const;
export type ProductOrderBy = (typeof ProductOrderBy)[keyof typeof ProductOrderBy];

/** Mirrors `Sheard.Type.OrderDirection`. */
export const OrderDirection = {
  Asc: 0,
  Desc: 1,
} as const;
export type OrderDirection = (typeof OrderDirection)[keyof typeof OrderDirection];

/** Mirrors the query string bound to `Domain.Products.ProductFilters`. */
export interface ProductQuery {
  readonly page: number;
  readonly pageSize: number;
  readonly name: string;
  readonly categoryId: string | null;
  readonly orderBy: ProductOrderBy;
  readonly orderByDirection: OrderDirection;
}

/** Mirrors `RestApi.Products.CreateProductRequest`. */
export interface CreateProductBody {
  readonly name: string;
  readonly description: string;
  readonly price: number;
  readonly quantity: number;
  readonly imagePath: string;
  readonly categoryIds: readonly string[];
}

/** Mirrors `RestApi.Products.UpdateProductRequest` - every field is optional. */
export type UpdateProductBody = Partial<CreateProductBody>;
