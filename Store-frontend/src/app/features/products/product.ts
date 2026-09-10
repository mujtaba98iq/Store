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

/** The order the storefront shows uploads in: primary first, then display order. */
function byProminence(left: ProductImage, right: ProductImage): number {
  return Number(right.isPrimary) - Number(left.isPrimary) || left.displayOrder - right.displayOrder;
}

/**
 * The uploaded image a replacement file should overwrite, or null when the product
 * has no upload yet and the file has to be posted as a new one instead.
 */
export function primaryImage(product: Product): ProductImage | null {
  return [...product.images].sort(byProminence).at(0) ?? null;
}

/**
 * The image to render: the uploaded primary image, then the remaining uploads by
 * display order, then the legacy `imagePath` column. Candidates that are not
 * absolute URLs are skipped rather than requested - relative paths point at files
 * the API no longer serves, so fetching them only produces a 404. Returns '' when
 * nothing is usable, and the card draws a placeholder instead.
 */
export function productImageUrl(product: Product): string {
  const uploaded = [...product.images].sort(byProminence).map((image) => image.imageUrl);

  return [...uploaded, product.imagePath].find(isFetchable)?.trim() ?? '';
}

/** Mirrors `RestApi.Extensions.FormFileExtensions`. */
export const MAX_IMAGE_SIZE_IN_MEGABYTES = 5;
const MAX_IMAGE_SIZE_IN_BYTES = MAX_IMAGE_SIZE_IN_MEGABYTES * 1024 * 1024;
const ALLOWED_IMAGE_TYPES: readonly string[] = [
  'image/jpeg',
  'image/jpg',
  'image/png',
  'image/webp',
];

/** What the file picker offers - the API enforces the same list. */
export const IMAGE_ACCEPT = ALLOWED_IMAGE_TYPES.join(',');

/**
 * Why `file` cannot be uploaded, or null when it can. The API checks the same
 * rules (plus the file signature); this only saves the round trip of an upload
 * that was never going to be accepted.
 */
export function imageFileError(file: File): string | null {
  const contentType = file.type.split(';')[0].trim().toLowerCase();
  if (!ALLOWED_IMAGE_TYPES.includes(contentType)) {
    return 'Choose a JPEG, PNG or WebP image.';
  }
  if (file.size === 0) {
    return 'That file is empty.';
  }
  if (file.size > MAX_IMAGE_SIZE_IN_BYTES) {
    return `Image cannot exceed ${MAX_IMAGE_SIZE_IN_MEGABYTES} MB.`;
  }
  return null;
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
