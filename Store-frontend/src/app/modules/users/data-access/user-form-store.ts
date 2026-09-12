import { Injectable, inject, signal } from '@angular/core';
import { EMPTY, Observable, catchError, finalize, tap } from 'rxjs';
import { ToastService } from '@app/core/services/common/toast';
import { FieldErrors, fieldErrors } from '@app/core/utils/api-error';
import { ApiUsersService } from '../api/users';
import { CreateUserBody, UpdateUserBody, User } from '../models/user.model';

/**
 * Saving an account: one call either way, plus whatever the API said about it.
 * The form component only collects the values and shows what comes back here.
 */
@Injectable()
export class UserFormStore {
  private readonly api = inject(ApiUsersService);
  private readonly toasts = inject(ToastService);

  private readonly savingState = signal(false);
  private readonly fieldErrorState = signal<FieldErrors>({});

  readonly saving = this.savingState.asReadonly();
  readonly serverFields = this.fieldErrorState.asReadonly();

  /**
   * Emits the saved account and announces it; a failed request emits nothing and
   * leaves the per-field messages in `serverFields`, so the dialog stays open with
   * the problem marked on the field it belongs to. Anything the API refused for a
   * reason that is not about one field - a username already taken, say - is
   * reported by `errorInterceptor`.
   */
  save(existing: User | null, fields: CreateUserBody): Observable<User> {
    this.savingState.set(true);
    this.fieldErrorState.set({});

    const request = existing
      ? this.api.update(existing.id, this.patchFor(fields))
      : this.api.create(fields);

    return request.pipe(
      tap(() =>
        this.toasts.success(existing ? 'User updated successfully.' : 'User created successfully.'),
      ),
      catchError((error: unknown) => {
        this.fieldErrorState.set(fieldErrors(error));
        return EMPTY;
      }),
      finalize(() => this.savingState.set(false)),
    );
  }

  /**
   * An untouched password box on an edit means "leave the password alone", so it is
   * left out of the patch rather than sent as an empty string - which the API's
   * validator would refuse, and which would otherwise blank out a working login.
   */
  private patchFor(fields: CreateUserBody): UpdateUserBody {
    const { username, role, password } = fields;
    return password ? { username, role, password } : { username, role };
  }
}
