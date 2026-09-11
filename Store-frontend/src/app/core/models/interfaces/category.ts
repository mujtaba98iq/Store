/** Mirrors `RestApi.Categories.CategoryResponse`. */
export interface Category {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
}
