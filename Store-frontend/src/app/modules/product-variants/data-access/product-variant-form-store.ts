import { Injectable, inject, signal } from '@angular/core';
import { EMPTY, Observable, catchError, finalize, tap } from 'rxjs';
import { ToastService } from '@app/core/services/common/toast';
import { FieldErrors, fieldErrors } from '@app/core/utils/api-error';
import { ApiProductVariantsService } from '../api/product-variants';
import {
  CreateProductVariantBody,
  ProductVariant,
  UpdateProductVariantBody,
} from '../models/product-variant.model';

/**
 * Saving a variant: one call either way, plus whatever the API said about it. The
 * form component only collects the fields and shows what comes back here.
 */
@Injectable()
export class ProductVariantFormStore {
  private readonly api = inject(ApiProductVariantsService);
  private readonly toasts = inject(ToastService);

  private readonly savingState = signal(false);
  private readonly fieldErrorState = signal<FieldErrors>({});

  readonly saving = this.savingState.asReadonly();
  readonly serverFields = this.fieldErrorState.asReadonly();

  /**
   * Emits the saved variant and announces it; a failed request emits nothing and
   * leaves the per-field messages in `serverFields`, so the dialog stays open with
   * the problem marked on the field it belongs to. Anything the API refused for a
   * reason that is not about one field - a SKU another variant already holds, say -
   * is reported by `errorInterceptor`.
   */
  save(
    existing: ProductVariant | null,
    fields: CreateProductVariantBody,
  ): Observable<ProductVariant> {
    this.savingState.set(true);
    this.fieldErrorState.set({});

    const request = existing
      ? this.api.update(existing.id, this.patchFor(fields))
      : this.api.create(fields);

    return request.pipe(
      tap(() =>
        this.toasts.success(
          existing
            ? 'Product variant updated successfully.'
            : 'Product variant created successfully.',
        ),
      ),
      catchError((error: unknown) => {
        this.fieldErrorState.set(fieldErrors(error));
        return EMPTY;
      }),
      finalize(() => this.savingState.set(false)),
    );
  }

  /**
   * The product a variant belongs to is fixed once it exists - the API's update
   * request has no field for it - so an edit sends everything but that.
   *
   * A price or barcode the admin cleared is left off rather than sent as null: the
   * API reads both the same way, so sending it would only look like it did
   * something. The form says as much where the boxes are.
   */
  private patchFor(fields: CreateProductVariantBody): UpdateProductVariantBody {
    return {
      sku: fields.sku,
      ...(fields.price !== undefined ? { price: fields.price } : {}),
      ...(fields.barcode !== undefined ? { barcode: fields.barcode } : {}),
      isActive: fields.isActive,
    };
  }
}
