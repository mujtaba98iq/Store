/**
 * The envelope every list endpoint answers with.
 * Mirrors `Sheard.Type.PaginationResult<T>`.
 */
export interface PaginatedResult<T> {
  readonly totalCount: number;
  readonly data: readonly T[];
}
