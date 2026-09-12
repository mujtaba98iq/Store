import { Injectable, inject, signal } from '@angular/core';
import { EMPTY, Observable, catchError, finalize } from 'rxjs';
import { CategoryDetail, CreateCategoryBody } from '@app/core/models/interfaces/category';
import { ApiCategoriesService } from '@app/core/services/api/categories';
import { FieldErrors, describeError, fieldErrors } from '@app/core/utils/api-error';

/**
 * Saving a category: one call either way, plus whatever the API said about it.
 * The form component only collects the values and shows what comes back here.
 */
@Injectable()
export class CategoryFormStore {
  private readonly api = inject(ApiCategoriesService);

  private readonly savingState = signal(false);
  private readonly errorState = signal<string | null>(null);
  private readonly fieldErrorState = signal<FieldErrors>({});

  readonly saving = this.savingState.asReadonly();
  readonly serverError = this.errorState.asReadonly();
  readonly serverFields = this.fieldErrorState.asReadonly();

  /**
   * Emits the saved category; a failed request emits nothing and leaves the
   * message in `serverError` / `serverFields`, so the dialog stays open.
   */
  save(existing: CategoryDetail | null, fields: CreateCategoryBody): Observable<CategoryDetail> {
    this.savingState.set(true);
    this.errorState.set(null);
    this.fieldErrorState.set({});

    const request = existing ? this.api.update(existing.id, fields) : this.api.create(fields);

    return request.pipe(
      catchError((error: unknown) => {
        this.errorState.set(describeError(error));
        this.fieldErrorState.set(fieldErrors(error));
        return EMPTY;
      }),
      finalize(() => this.savingState.set(false)),
    );
  }
}
