import { NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductCard } from '@app/shared/components/product-card/product-card';
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
}
