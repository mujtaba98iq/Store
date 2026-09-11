import { Injectable, computed, inject, signal } from '@angular/core';
import { OrderDirection } from '@app/core/models/enums/order-direction';
import { ApiProductsService } from '@app/modules/products/api/products';
import { Product, ProductOrderBy } from '@app/modules/products/models/product.model';

/**
 * The one product the landing hero shows. Reads the products resource rather
 * than owning an endpoint of its own, so the hero and the catalogue always
 * agree on what "newest" means.
 */
@Injectable()
export class FeaturedProductStore {
  private readonly api = inject(ApiProductsService);

  private readonly newest = this.api.list(
    signal({
      page: 1,
      pageSize: 1,
      name: '',
      categoryId: null,
      orderBy: ProductOrderBy.CreatedAt,
      orderByDirection: OrderDirection.Desc,
    }),
  );

  readonly featured = computed<Product | null>(() =>
    this.newest.hasValue() ? (this.newest.value()?.data[0] ?? null) : null,
  );
}
