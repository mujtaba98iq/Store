import { Injectable, inject, signal } from '@angular/core';
import { EMPTY, Observable, catchError, finalize, map, of, switchMap, tap } from 'rxjs';
import { ToastService } from '@app/core/services/common/toast';
import { FieldErrors, fieldErrors } from '@app/core/utils/api-error';
import { ErrorNotification } from '@app/core/utils/error-notification';
import { ApiProductImagesService } from '../api/product-images';
import { ApiProductsService } from '../api/products';
import { CreateProductBody, Product } from '../models/product.model';
import { primaryImage } from '../utils/product-image';

/** Everything except the image path, which is filled in from the upload. */
export type ProductFields = Omit<CreateProductBody, 'imagePath'>;

/**
 * Saving a product: the product row, its image upload and the order the two have
 * to happen in. The form component only collects the values and shows what comes
 * back here.
 */
@Injectable()
export class ProductFormStore {
  private readonly api = inject(ApiProductsService);
  private readonly imagesApi = inject(ApiProductImagesService);
  private readonly toasts = inject(ToastService);

  private readonly savingState = signal(false);
  private readonly fieldErrorState = signal<FieldErrors>({});

  /**
   * The product this form created, so retrying after a failed upload edits that
   * product instead of creating a second one.
   */
  private readonly createdState = signal<Product | null>(null);

  readonly saving = this.savingState.asReadonly();
  readonly serverFields = this.fieldErrorState.asReadonly();
  readonly created = this.createdState.asReadonly();

  /**
   * Emits the saved product and announces it; a failed request emits nothing and
   * leaves the per-field messages in `serverFields`, so the dialog stays open with
   * the problem marked on the field it belongs to. Anything the API refused for a
   * reason that is not about one field is reported by `errorInterceptor`.
   */
  save(existing: Product | null, fields: ProductFields, file: File | null): Observable<Product> {
    // A retry after a failed upload is handed the product this store created, and
    // is still the creation the admin started - not an edit of something that was
    // already in the catalogue.
    const isCreate = existing === null || existing.id === this.createdState()?.id;

    this.savingState.set(true);
    this.fieldErrorState.set({});

    return this.request(existing, fields, file).pipe(
      tap(() =>
        this.toasts.success(
          isCreate ? 'Product created successfully.' : 'Product updated successfully.',
        ),
      ),
      catchError((error: unknown) => {
        this.fieldErrorState.set(fieldErrors(error));
        return EMPTY;
      }),
      finalize(() => this.savingState.set(false)),
    );
  }

  private request(
    existing: Product | null,
    fields: ProductFields,
    file: File | null,
  ): Observable<Product> {
    if (file) {
      return existing
        ? this.saveWithNewImage(existing, fields, file)
        : this.createWithImage(fields, file);
    }

    // An edit keeps the image it already has; a new product has no fallback, and
    // the form refuses to submit without one.
    return existing ? this.api.update(existing.id, fields) : EMPTY;
  }

  /**
   * Creating takes three calls: `ImagePath` is required up front, the upload needs
   * a product id that only exists once the row does, and the stored URL is only
   * known once the file is up. The file name stands in until the last call
   * replaces it with the URL the image was stored at.
   */
  private createWithImage(fields: ProductFields, file: File): Observable<Product> {
    return this.api.create({ ...fields, imagePath: file.name }).pipe(
      tap((product) => this.createdState.set(product)),
      switchMap((product) =>
        this.uploadPrimary(product, file).pipe(
          switchMap((imageUrl) =>
            // The product and its image are both stored by now, and `imagePath` is
            // only the fallback for readers that ignore `images` - not worth
            // sending the admin back into the form over, so it is not worth a
            // toast either.
            this.api
              .update(product.id, { imagePath: imageUrl }, ErrorNotification.Silent)
              .pipe(catchError(() => of(product))),
          ),
        ),
      ),
    );
  }

  /** An existing product uploads first, so the new URL rides along with the other edits. */
  private saveWithNewImage(
    product: Product,
    fields: ProductFields,
    file: File,
  ): Observable<Product> {
    return this.uploadPrimary(product, file).pipe(
      switchMap((imageUrl) => this.api.update(product.id, { ...fields, imagePath: imageUrl })),
    );
  }

  /** Stores `file` as the product's primary image and returns the URL it landed on. */
  private uploadPrimary(product: Product, file: File): Observable<string> {
    const current = primaryImage(product);
    const upload = current
      ? this.imagesApi.update(current.id, file, true)
      : this.imagesApi.create({ productId: product.id, file, isPrimary: true, displayOrder: 1 });

    return upload.pipe(map((image) => image.imageUrl));
  }
}
