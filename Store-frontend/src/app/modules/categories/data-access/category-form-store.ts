import { Injectable, inject, signal } from '@angular/core';
import { EMPTY, Observable, catchError, finalize, tap } from 'rxjs';
import { CategoryDetail, CreateCategoryBody } from '@app/core/models/interfaces/category';
import { ApiCategoriesService } from '@app/core/services/api/categories';
import { ToastService } from '@app/core/services/common/toast';
import { FieldErrors, fieldErrors } from '@app/core/utils/api-error';

/**
 * Saving a category: one call either way, plus whatever the API said about it.
 * The form component only collects the values and shows what comes back here.
 */
@Injectable()
export class CategoryFormStore {
  private readonly api = inject(ApiCategoriesService);
  private readonly toasts = inject(ToastService);

  private readonly savingState = signal(false);
  private readonly fieldErrorState = signal<FieldErrors>({});

  readonly saving = this.savingState.asReadonly();
  readonly serverFields = this.fieldErrorState.asReadonly();

  /**
   * Emits the saved category and announces it; a failed request emits nothing and
   * leaves the per-field messages in `serverFields`, so the dialog stays open with
   * the problem marked on the field it belongs to. Anything the API refused for a
   * reason that is not about one field is reported by `errorInterceptor`.
   */
  save(existing: CategoryDetail | null, fields: CreateCategoryBody): Observable<CategoryDetail> {
    this.savingState.set(true);
    this.fieldErrorState.set({});

    const request = existing ? this.api.update(existing.id, fields) : this.api.create(fields);

    return request.pipe(
      tap(() =>
        this.toasts.success(
          existing ? 'Category updated successfully.' : 'Category created successfully.',
        ),
      ),
      catchError((error: unknown) => {
        this.fieldErrorState.set(fieldErrors(error));
        return EMPTY;
      }),
      finalize(() => this.savingState.set(false)),
    );
  }
}
