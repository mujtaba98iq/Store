import { NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductCard } from '../../shared/components/product-card/product-card';
import { OrderDirection, Product, ProductOrderBy, productImageUrl } from '../products/product';
import { productsResource } from '../products/products-api';
import { AvatarStack } from './components/avatar-stack/avatar-stack';

@Component({
  selector: 'app-home',
  imports: [NgOptimizedImage, RouterLink, AvatarStack, ProductCard],
  templateUrl: './home.html',
  styleUrl: './home.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Home {
  /** The hero shows the newest product the catalogue has, image and all. */
  private readonly newest = productsResource(
    signal({
      page: 1,
      pageSize: 1,
      name: '',
      categoryId: null,
      orderBy: ProductOrderBy.CreatedAt,
      orderByDirection: OrderDirection.Desc,
    }),
  );

  protected readonly featured = computed<Product | null>(() =>
    this.newest.hasValue() ? (this.newest.value()?.data[0] ?? null) : null,
  );

  protected readonly imageFor = productImageUrl;

  protected readonly avatars = signal<readonly string[]>([
    'images/avatar-1.svg',
    'images/avatar-2.svg',
    'images/avatar-3.svg',
    'images/avatar-4.svg',
  ]);

  protected readonly customerCount = signal('60k');
}
