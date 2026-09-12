import { OrderDirection } from '@app/core/models/enums/order-direction';

/**
 * Mirrors `RestApi.Inventories.InventoryResponse`.
 *
 * One row per product variant: how many units exist, how many of them are already
 * spoken for, and what is left to sell.
 */
export interface Inventory {
  readonly id: string;
  readonly productVariantId: string;
  /**
   * The SKU of the variant this row stocks. A stock row has no name of its own, so
   * this is what labels it everywhere. Null only if the variant behind it is gone.
   */
  readonly sku: string | null;
  /** Units on hand, reserved ones included. */
  readonly quantity: number;
  /** Units already promised to an order, still on the shelf. */
  readonly reservedQuantity: number;
  /** `quantity - reservedQuantity`, computed by the API rather than here. */
  readonly availableQuantity: number;
  readonly createdAt: string;
  readonly updatedAt: string | null;
  readonly createdById: string;
  readonly updatedById: string | null;
}

/** Mirrors `Domain.Inventories.InventoryOrderBy`. */
export const InventoryOrderBy = {
  CreatedAt: 1,
  Quantity: 2,
  ReservedQuantity: 3,
  AvailableQuantity: 4,
  UpdatedAt: 5,
  Sku: 6,
} as const;
export type InventoryOrderBy = (typeof InventoryOrderBy)[keyof typeof InventoryOrderBy];

/**
 * Mirrors the query string bound to `Domain.Inventories.InventoryFilters`.
 *
 * Every filter the API applies is ANDed with the rest, so a typed SKU and a chosen
 * availability narrow each other rather than widening the list.
 */
export interface InventoryQuery {
  readonly page: number;
  readonly pageSize: number;
  /** Partial match against the variant's SKU. Empty means no SKU filter. */
  readonly sku: string;
  /** `null` is every row; true only what is left to sell, false only the sold out. */
  readonly isAvailable: boolean | null;
  /** Bounds on units on hand. `null` is no bound at that end. */
  readonly minQuantity: number | null;
  readonly maxQuantity: number | null;
  readonly orderBy: InventoryOrderBy;
  readonly orderByDirection: OrderDirection;
}

/** Mirrors `RestApi.Inventories.CreateInventoryRequest`. */
export interface CreateInventoryBody {
  readonly productVariantId: string;
  readonly quantity: number;
  readonly reservedQuantity: number;
}

/**
 * Mirrors `RestApi.Inventories.UpdateInventoryRequest`. Only the counts can be
 * edited - the variant a row stocks is fixed for its lifetime.
 */
export interface UpdateInventoryBody {
  readonly quantity?: number;
  readonly reservedQuantity?: number;
}
