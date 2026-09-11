/** Mirrors `Sheard.Type.OrderDirection`. */
export const OrderDirection = {
  Asc: 0,
  Desc: 1,
} as const;
export type OrderDirection = (typeof OrderDirection)[keyof typeof OrderDirection];
