/** Mirrors `Domain.Categories.CategoryOrderBy`. */
export const CategoryOrderBy = {
  CreatedAt: 1,
  Name: 2,
} as const;
export type CategoryOrderBy = (typeof CategoryOrderBy)[keyof typeof CategoryOrderBy];
