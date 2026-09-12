import { OrderDirection } from '@app/core/models/enums/order-direction';

/**
 * Mirrors `RestApi.ProductVariants.ProductVariantResponse`.
 *
 * One sellable form of a product - a size, a shade - identified by its own SKU. It
 * can carry its own price and barcode, and it is what a cart line and a stock row
 * actually point at; the product above it is only the thing being varied.
 */
export interface ProductVariant {
  readonly id: string;
  readonly productId: string;
  /**
   * The name of the product this variant varies. The API carries it alongside the
   * variant so a listing does not have to fetch every product to label its rows.
   * Null only if the product behind it is gone.
   */
  readonly productName: string | null;
  readonly sku: string;
  /** Null means the variant has no price of its own and sells at the product's. */
  readonly price: number | null;
  readonly barcode: string | null;
  /** An inactive variant stays on the books but cannot be put in a cart. */
  readonly isActive: boolean;
  readonly createdAt: string;
  readonly updatedAt: string | null;
  readonly createdById: string;
  readonly updatedById: string | null;
}

/** Mirrors `Domain.ProductVariants.ProductVariantOrderBy`. */
export const ProductVariantOrderBy = {
  CreatedAt: 1,
  Sku: 2,
  Price: 3,
} as const;
export type ProductVariantOrderBy =
  (typeof ProductVariantOrderBy)[keyof typeof ProductVariantOrderBy];

/**
 * Mirrors the query string bound to `Domain.ProductVariants.ProductVariantFilters`.
 *
 * Every filter the API applies is ANDed with the rest, so a typed SKU and a chosen
 * product narrow each other rather than widening the list.
 */
export interface ProductVariantQuery {
  readonly page: number;
  readonly pageSize: number;
  /** Partial match against the SKU. Empty means no SKU filter. */
  readonly sku: string;
  /** Partial match against the barcode. Empty means no barcode filter. */
  readonly barcode: string;
  /** `null` is every product; an ID narrows to that product's variants. */
  readonly productId: string | null;
  /** `null` is every variant; true only the active ones, false only the inactive. */
  readonly isActive: boolean | null;
  readonly orderBy: ProductVariantOrderBy;
  readonly orderByDirection: OrderDirection;
}

/** Mirrors `RestApi.ProductVariants.CreateProductVariantRequest`. */
export interface CreateProductVariantBody {
  readonly productId: string;
  readonly sku: string;
  /** Omitted when null, so the variant sells at the product's price. */
  readonly price?: number;
  readonly barcode?: string;
  readonly isActive: boolean;
}

/**
 * Mirrors `RestApi.ProductVariants.UpdateProductVariantRequest`. The product a
 * variant belongs to is fixed for its lifetime, so it has no field here.
 *
 * A field left off is one the API leaves alone. It reads `null` the same way, so
 * there is currently no request that clears a price or a barcode back to empty -
 * see the hints on the form, which say so rather than pretending otherwise.
 */
export interface UpdateProductVariantBody {
  readonly sku?: string;
  readonly price?: number;
  readonly barcode?: string;
  readonly isActive?: boolean;
}
