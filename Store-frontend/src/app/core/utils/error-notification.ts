import { HttpContext, HttpContextToken } from '@angular/common/http';

/**
 * What `errorInterceptor` does when a request fails. Set per request by the API
 * client that issues it, so the decision sits next to the endpoint rather than
 * being re-made in every component.
 */
export const ErrorNotification = {
  /** The default: the reader is told in a toast. */
  Toast: 'toast',
  /**
   * The caller renders the API's per-field messages beside the inputs they belong
   * to. Only a failure with no field messages to show there is worth a toast.
   */
  FieldsInline: 'fields-inline',
  /**
   * Nothing is shown. For work the reader did not ask for, and for the requests
   * whose own screen already reports the failure with somewhere to go next.
   */
  Silent: 'silent',
} as const;
export type ErrorNotification = (typeof ErrorNotification)[keyof typeof ErrorNotification];

export const ERROR_NOTIFICATION = new HttpContextToken<ErrorNotification>(
  () => ErrorNotification.Toast,
);

/** Builds the context to hand to `HttpClient`, or to an `httpResource` request. */
export function errorNotification(policy: ErrorNotification): HttpContext {
  return new HttpContext().set(ERROR_NOTIFICATION, policy);
}
