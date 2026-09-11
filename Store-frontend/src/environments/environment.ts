/**
 * Default configuration, used by `ng serve` and the development build.
 *
 * `apiBaseUrl` stays relative so `proxy.conf.json` forwards `/api` to the Store
 * API running on http://localhost:5127.
 */
export const environment = {
  production: false,
  apiBaseUrl: '/api',
};
