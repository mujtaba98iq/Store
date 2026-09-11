/**
 * Production build, swapped in by the `fileReplacements` entry in angular.json.
 *
 * The base URL stays relative, as it is in development: whatever serves the
 * built app is expected to expose the Store API under the same `/api` path.
 */
export const environment = {
  production: true,
  apiBaseUrl: '/api',
};
