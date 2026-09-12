import { OrderDirection } from '@app/core/models/enums/order-direction';
import { Category } from '@app/core/models/interfaces/category';
import { ProductVariant } from './product-variant.model';

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
  /**
   * The forms this product is sold in, carried by the API alongside the product
   * so a product page does not have to fetch them separately - and so a visitor
   * without a token can see them, which the variants endpoint would not allow.
   * Live variants, inactive ones included: what is purchasable is filtered here.
   */
  readonly variants: readonly ProductVariant[];
  readonly images: readonly ProductImage[];
}

/** Mirrors `Domain.Products.ProductOrderBy`. */
export const ProductOrderBy = {
  CreatedAt: 1,
  Name: 2,
  Price: 3,
} as const;
export type ProductOrderBy = (typeof ProductOrderBy)[keyof typeof ProductOrderBy];

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

/** The multipart fields `RestApi.ProductImages.CreateProductImageRequest` binds. */
export interface NewProductImage {
  readonly productId: string;
  readonly file: File;
  readonly isPrimary: boolean;
  readonly displayOrder: number;
}
