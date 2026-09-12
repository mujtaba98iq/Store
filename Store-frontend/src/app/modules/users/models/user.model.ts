import { OrderDirection } from '@app/core/models/enums/order-direction';

/**
 * The roles the API's `[Authorize(Roles = ...)]` attributes recognise. The column
 * behind them is free text, so anything else the database happens to hold is still
 * listed - it just has no pill of its own.
 */
export const UserRole = {
  Admin: 'Admin',
  User: 'User',
} as const;
export type UserRole = (typeof UserRole)[keyof typeof UserRole];

/** Mirrors `RestApi.Users.UserResponse`. The password hash never leaves the API. */
export interface User {
  readonly id: string;
  readonly username: string;
  readonly role: string;
  readonly createdAt: string;
  readonly updatedAt: string | null;
  readonly createdById: string;
  readonly updatedById: string | null;
}

/** Mirrors `Domain.Users.UserOrderBy`. */
export const UserOrderBy = {
  CreatedAt: 1,
  Username: 2,
} as const;
export type UserOrderBy = (typeof UserOrderBy)[keyof typeof UserOrderBy];

/**
 * Mirrors the query string bound to `Domain.Users.UserFilters`.
 *
 * `username` and `role` are two separate `LIKE` filters the API ANDs together, so
 * a typed term and a chosen role narrow each other rather than widening the list.
 */
export interface UserQuery {
  readonly page: number;
  readonly pageSize: number;
  readonly username: string;
  /** Empty means every role. */
  readonly role: string;
  readonly orderBy: UserOrderBy;
  readonly orderByDirection: OrderDirection;
}

/** Mirrors `RestApi.Users.CreateUserRequest`. */
export interface CreateUserBody {
  readonly username: string;
  readonly password: string;
  readonly role: string;
}

/** Mirrors `RestApi.Users.UpdateUserRequest` - every field is optional. */
export type UpdateUserBody = Partial<CreateUserBody>;
