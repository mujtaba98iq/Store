import { Product, ProductImage } from '../models/product.model';

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
