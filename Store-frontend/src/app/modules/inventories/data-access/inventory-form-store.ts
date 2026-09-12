import { Injectable, inject, signal } from '@angular/core';
import { EMPTY, Observable, catchError, finalize, tap } from 'rxjs';
import { ToastService } from '@app/core/services/common/toast';
import { FieldErrors, fieldErrors } from '@app/core/utils/api-error';
import { ApiInventoriesService } from '../api/inventories';
import { CreateInventoryBody, Inventory, UpdateInventoryBody } from '../models/inventory.model';

/**
 * Saving a stock row: one call either way, plus whatever the API said about it.
 * The form component only collects the counts and shows what comes back here.
 */
@Injectable()
export class InventoryFormStore {
  private readonly api = inject(ApiInventoriesService);
  private readonly toasts = inject(ToastService);

  private readonly savingState = signal(false);
  private readonly fieldErrorState = signal<FieldErrors>({});

  readonly saving = this.savingState.asReadonly();
  readonly serverFields = this.fieldErrorState.asReadonly();

  /**
   * Emits the saved row and announces it; a failed request emits nothing and leaves
   * the per-field messages in `serverFields`, so the dialog stays open with the
   * problem marked on the field it belongs to. Anything the API refused for a reason
   * that is not about one field - a variant that is already stocked, say - is
   * reported by `errorInterceptor`.
   */
  save(existing: Inventory | null, fields: CreateInventoryBody): Observable<Inventory> {
    this.savingState.set(true);
    this.fieldErrorState.set({});

    const request = existing
      ? this.api.update(existing.id, this.patchFor(fields))
      : this.api.create(fields);

    return request.pipe(
      tap(() =>
        this.toasts.success(
          existing ? 'Stock updated successfully.' : 'Stock row created successfully.',
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
   * The variant a row stocks is fixed once it exists - the API's update request has
   * no field for it - so an edit sends only the two counts.
   */
  private patchFor(fields: CreateInventoryBody): UpdateInventoryBody {
    return { quantity: fields.quantity, reservedQuantity: fields.reservedQuantity };
  }
}
