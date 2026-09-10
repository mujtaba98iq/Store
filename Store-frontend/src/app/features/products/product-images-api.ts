import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ProductImage } from './product';

const PRODUCT_IMAGES_URL = '/api/ProductImages';

/** The multipart fields `RestApi.ProductImages.CreateProductImageRequest` binds. */
export interface NewProductImage {
  readonly productId: string;
  readonly file: File;
  readonly isPrimary: boolean;
  readonly displayOrder: number;
}

/**
 * Both endpoints take `multipart/form-data`, so every request goes out as a
 * `FormData` body - the Content-Type header is left unset on purpose, the
 * browser has to add it along with the multipart boundary.
 */
@Injectable({ providedIn: 'root' })
export class ProductImagesApi {
  private readonly http = inject(HttpClient);

  create(image: NewProductImage): Observable<ProductImage> {
    const body = new FormData();
    body.set('ProductId', image.productId);
    body.set('Image', image.file, image.file.name);
    body.set('IsPrimary', String(image.isPrimary));
    body.set('DisplayOrder', String(image.displayOrder));

    return this.http.post<ProductImage>(PRODUCT_IMAGES_URL, body);
  }

  /** Replaces the file behind an existing image; the old asset is deleted by the API. */
  update(id: string, file: File, isPrimary: boolean): Observable<ProductImage> {
    const body = new FormData();
    body.set('Image', file, file.name);
    body.set('IsPrimary', String(isPrimary));

    return this.http.patch<ProductImage>(`${PRODUCT_IMAGES_URL}/${id}`, body);
  }
}
