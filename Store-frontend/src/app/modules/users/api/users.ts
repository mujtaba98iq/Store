import { HttpClient, httpResource } from '@angular/common/http';
import { Injectable, Injector, Signal, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PaginatedResult } from '@app/core/models/interfaces/api-response';
import { ErrorNotification, errorNotification } from '@app/core/utils/error-notification';
import { environment } from '@env/environment';
import { CreateUserBody, UpdateUserBody, User, UserQuery } from '../models/user.model';

const USERS_URL = `${environment.apiBaseUrl}/users`;

function toParams(query: UserQuery): Record<string, string | number> {
  const params: Record<string, string | number> = {
    Page: query.page,
    PageSize: query.pageSize,
    OrderBy: query.orderBy,
    OrderByDirection: query.orderByDirection,
  };

  // Omitted rather than sent empty, so the API skips the filter entirely.
  const username = query.username.trim();
  if (username) {
    params['Username'] = username;
  }

  if (query.role) {
    params['Role'] = query.role;
  }

  return params;
}

/**
 * One method per endpoint of the API's users resource. Every one of them is behind
 * the Admin role, so this client is only ever reached from the management page.
 */
@Injectable({ providedIn: 'root' })
export class ApiUsersService {
  private readonly http = inject(HttpClient);
  private readonly injector = inject(Injector);

  /**
   * Reactive management listing - refetches whenever `query` changes. A failed read
   * is reported by the page that asked for it, which can offer a retry; a toast
   * would only say the same thing twice.
   */
  search(query: Signal<UserQuery>) {
    return httpResource<PaginatedResult<User>>(
      () => ({
        url: USERS_URL,
        params: toParams(query()),
        context: errorNotification(ErrorNotification.Silent),
      }),
      { injector: this.injector },
    );
  }

  create(body: CreateUserBody): Observable<User> {
    return this.http.post<User>(USERS_URL, body, {
      context: errorNotification(ErrorNotification.FieldsInline),
    });
  }

  update(id: string, body: UpdateUserBody): Observable<User> {
    return this.http.patch<User>(`${USERS_URL}/${id}`, body, {
      context: errorNotification(ErrorNotification.FieldsInline),
    });
  }

  /** Soft-deletes the account; the API answers 204 with no body. */
  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${USERS_URL}/${id}`);
  }
}
