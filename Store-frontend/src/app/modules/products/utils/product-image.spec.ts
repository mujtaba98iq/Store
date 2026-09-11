import { Product, ProductImage } from '../models/product.model';
import { productImageUrl } from './product-image';

const CLOUDINARY = 'https://res.cloudinary.com/lvkdjyco/image/upload/v1/store/a.webp';

function product(overrides: Partial<Product> = {}): Product {
  return {
    id: 'p1',
    name: 'Overnight Recovery Tube',
    description: 'A ceramide-rich sleeping mask.',
    price: 59,
    quantity: 20,
    imagePath: '/product-images/product-tube.svg',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    categories: [],
    images: [],
    ...overrides,
  };
}

function image(overrides: Partial<ProductImage> = {}): ProductImage {
  return {
    id: 'i1',
    productId: 'p1',
    imageUrl: CLOUDINARY,
    isPrimary: false,
    displayOrder: 0,
    ...overrides,
  };
}

describe('productImageUrl', () => {
  it('prefers an uploaded image over imagePath', () => {
    expect(productImageUrl(product({ images: [image()] }))).toBe(CLOUDINARY);
  });

  it('prefers the primary image over display order', () => {
    const images = [
      image({ id: 'i1', imageUrl: 'https://cdn.example/first.webp', displayOrder: 0 }),
      image({
        id: 'i2',
        imageUrl: 'https://cdn.example/primary.webp',
        isPrimary: true,
        displayOrder: 9,
      }),
    ];

    expect(productImageUrl(product({ images }))).toBe('https://cdn.example/primary.webp');
  });

  it('uses the lowest display order when none is primary', () => {
    const images = [
      image({ id: 'i1', imageUrl: 'https://cdn.example/late.webp', displayOrder: 5 }),
      image({ id: 'i2', imageUrl: 'https://cdn.example/early.webp', displayOrder: 1 }),
    ];

    expect(productImageUrl(product({ images }))).toBe('https://cdn.example/early.webp');
  });

  it('uses an absolute imagePath when nothing has been uploaded', () => {
    expect(productImageUrl(product({ imagePath: 'https://cdn.example/legacy.png' }))).toBe(
      'https://cdn.example/legacy.png',
    );
  });

  // The requests these would produce are exactly the 404s we want to stop making.
  it('returns nothing for a relative imagePath', () => {
    expect(productImageUrl(product({ imagePath: '/product-images/product-tube.svg' }))).toBe('');
    expect(productImageUrl(product({ imagePath: 'images/product-serum.svg' }))).toBe('');
    expect(productImageUrl(product({ imagePath: 'image.jpg' }))).toBe('');
  });

  it('skips a relative uploaded url rather than requesting it', () => {
    const images = [image({ imageUrl: 'IMageURL.png', isPrimary: true })];

    expect(productImageUrl(product({ images }))).toBe('');
  });

  it('falls through a relative upload to an absolute imagePath', () => {
    const images = [image({ imageUrl: 'IMageURL.png', isPrimary: true })];

    expect(productImageUrl(product({ images, imagePath: CLOUDINARY }))).toBe(CLOUDINARY);
  });

  it('does not mutate the images array while sorting', () => {
    const images = [image({ id: 'i1', displayOrder: 5 }), image({ id: 'i2', displayOrder: 1 })];
    productImageUrl(product({ images }));

    expect(images.map((i) => i.id)).toEqual(['i1', 'i2']);
  });
});
