import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap } from 'rxjs';
import { ToastService } from '@app/core/services/common/toast';
import { describeError, fieldErrors } from '@app/core/utils/api-error';
import { ERROR_NOTIFICATION, ErrorNotification } from '@app/core/utils/error-notification';

/**
 * A refused request is a warning rather than an error when the reader can fix it
 * by waiting or by signing in again; everything else has actually gone wrong.
 */
function isRecoverable(error: HttpErrorResponse): boolean {
  return error.status === 401 || error.status === 429;
}

/**
 * The one place a failed request is reported. Every status the API can answer
 * with - 400, 401, 403, 404, 409, 422, 500, a dead connection - arrives here,
 * is turned into a sentence by `describeError`, and is shown once.
 *
 * `describeError` prefers whatever the API said about this specific request, so a
 * validation or business message reaches the reader intact; only a response that
 * carries nothing usable falls back to a canned sentence. Nothing technical is
 * ever shown - the body is read for known message fields and never for a stack.
 *
 * Register it outside `authInterceptor`, so a 401 that the refresh recovers from
 * is never reported.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const policy = request.context.get(ERROR_NOTIFICATION);
  if (policy === ErrorNotification.Silent) {
    return next(request);
  }

  const toasts = inject(ToastService);

  return next(request).pipe(
    tap({
      error: (error: unknown) => {
        // The form shows these beside the field they belong to; a toast saying the
        // same thing again is the second notification for one failed request.
        if (policy === ErrorNotification.FieldsInline && Object.keys(fieldErrors(error)).length) {
          return;
        }

        const message = describeError(error);
        if (!message) {
          return;
        }

        if (error instanceof HttpErrorResponse && isRecoverable(error)) {
          toasts.warning(message);
          return;
        }

        toasts.error(message);
      },
    }),
  );
};
