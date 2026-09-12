import { NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ProductCard } from '@app/shared/components/product-card/product-card';
import { Product } from '@app/modules/products/models/product.model';
import { productImageUrl } from '@app/modules/products/utils/product-image';
import { FeaturedProductStore } from '../../data-access/featured-product-store';
import { AvatarStack } from '../../components/avatar-stack/avatar-stack';

@Component({
  selector: 'app-home',
  imports: [NgOptimizedImage, RouterLink, AvatarStack, ProductCard],
  templateUrl: './home.html',
  styleUrl: './home.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [FeaturedProductStore],
})
export class Home {
  private readonly store = inject(FeaturedProductStore);
  private readonly router = inject(Router);

  /** The hero shows the newest product the catalogue has, image and all. */
  protected readonly featured = this.store.featured;

  protected readonly imageFor = productImageUrl;

  protected readonly avatars = signal<readonly string[]>([
    'images/avatar-1.svg',
    'images/avatar-2.svg',
    'images/avatar-3.svg',
    'images/avatar-4.svg',
  ]);

  protected readonly customerCount = signal('60k');

  /**
   * A product is not buyable on its own - a variant is what goes in a cart - so
   * the hero card opens the product page, where the size and the count are
   * chosen. The same move the catalogue's cards make.
   */
  protected openDetails(product: Product): void {
    void this.router.navigate(['/products', product.id]);
  }
}
