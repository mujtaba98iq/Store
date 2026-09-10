import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Category, Product, ProductImage } from '../../product';
import { ProductForm } from './product-form';

const MOISTURISERS: Category = { id: 'c1', name: 'Moisturisers', description: null };
const STORED_URL = 'https://res.cloudinary.com/lvkdjyco/image/upload/v1/store/cream.webp';

function product(overrides: Partial<Product> = {}): Product {
  return {
    id: 'p1',
    name: 'Moisture Repair Cream',
    description: 'Rich barrier cream.',
    price: 67,
    quantity: 4,
    imagePath: 'cream.png',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    categories: [MOISTURISERS],
    images: [],
    ...overrides,
  };
}

function uploaded(overrides: Partial<ProductImage> = {}): ProductImage {
  return {
    id: 'i1',
    productId: 'p1',
    imageUrl: STORED_URL,
    isPrimary: true,
    displayOrder: 1,
    ...overrides,
  };
}

function imageFile(name = 'cream.png', type = 'image/png'): File {
  return new File(['binary'], name, { type });
}

describe('ProductForm', () => {
  let fixture: ComponentFixture<ProductForm>;
  let element: HTMLElement;
  let http: HttpTestingController;

  const fill = (selector: string, value: string) => {
    const field = element.querySelector<HTMLInputElement>(selector);
    field!.value = value;
    field!.dispatchEvent(new Event('input'));
  };

  const pick = (file: File | null) => {
    const picker = element.querySelector<HTMLInputElement>('#product-image');
    Object.defineProperty(picker, 'files', { value: file ? [file] : [], configurable: true });
    picker!.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  };

  const submit = () => {
    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  };

  const errors = () =>
    Array.from(element.querySelectorAll('.form__hint--error')).map((node) =>
      node.textContent?.trim(),
    );

  /** Fills in everything but the image, which each test decides on. */
  const fillDetails = () => {
    fill('#product-name', 'Moisture Repair Cream');
    const description = element.querySelector<HTMLTextAreaElement>('#product-description');
    description!.value = 'Rich barrier cream.';
    description!.dispatchEvent(new Event('input'));
    fill('#product-price', '67');
    fill('#product-quantity', '4');
  };

  const start = (existing: Product | null = null) => {
    fixture = TestBed.createComponent(ProductForm);
    element = fixture.nativeElement as HTMLElement;
    fixture.componentRef.setInput('product', existing);
    fixture.componentRef.setInput('categories', [MOISTURISERS]);
    fixture.detectChanges();
  };

  beforeEach(async () => {
    // jsdom has no object-URL support, and the preview asks for one per file.
    URL.createObjectURL = () => 'blob:preview';
    URL.revokeObjectURL = () => undefined;

    await TestBed.configureTestingModule({
      imports: [ProductForm],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify({ ignoreCancelled: true });
  });

  it('creates the product, uploads the file, then stores the URL it came back with', () => {
    start();
    fillDetails();
    pick(imageFile());
    submit();

    // The upload needs an id, so the product is written first with the file
    // name standing in for the URL that does not exist yet.
    const create = http.expectOne((request) => request.url === '/api/products');
    expect(create.request.method).toBe('POST');
    expect(create.request.body).toMatchObject({
      name: 'Moisture Repair Cream',
      price: 67,
      quantity: 4,
      imagePath: 'cream.png',
      categoryIds: [],
    });
    create.flush(product());

    const upload = http.expectOne((request) => request.url === '/api/ProductImages');
    expect(upload.request.method).toBe('POST');
    const body = upload.request.body as FormData;
    expect(body.get('ProductId')).toBe('p1');
    expect(body.get('IsPrimary')).toBe('true');
    expect((body.get('Image') as File).name).toBe('cream.png');
    // The browser has to add the multipart boundary, so nothing may set it here.
    expect(upload.request.headers.has('Content-Type')).toBe(false);
    upload.flush(uploaded());

    const saved: Product[] = [];
    fixture.componentInstance.saved.subscribe((value) => saved.push(value));

    const patch = http.expectOne((request) => request.url === '/api/products/p1');
    expect(patch.request.method).toBe('PATCH');
    expect(patch.request.body).toEqual({ imagePath: STORED_URL });
    patch.flush(product({ imagePath: STORED_URL }));

    expect(saved.map((value) => value.imagePath)).toEqual([STORED_URL]);
  });

  it('replaces the file behind an existing image and saves the URL with the edit', () => {
    start(product({ images: [uploaded()] }));
    pick(imageFile('replacement.webp', 'image/webp'));
    submit();

    const upload = http.expectOne((request) => request.url === '/api/ProductImages/i1');
    expect(upload.request.method).toBe('PATCH');
    expect(((upload.request.body as FormData).get('Image') as File).name).toBe('replacement.webp');
    upload.flush(uploaded({ imageUrl: 'https://cdn.example/replacement.webp' }));

    // One PATCH carries both the fields and the URL the upload returned.
    const patch = http.expectOne((request) => request.url === '/api/products/p1');
    expect(patch.request.body).toMatchObject({
      name: 'Moisture Repair Cream',
      imagePath: 'https://cdn.example/replacement.webp',
      categoryIds: ['c1'],
    });
    patch.flush(product());
  });

  it('posts a first image for a product that has none', () => {
    start(product());
    pick(imageFile());
    submit();

    const upload = http.expectOne((request) => request.url === '/api/ProductImages');
    expect(upload.request.method).toBe('POST');
    upload.flush(uploaded());

    http.expectOne((request) => request.url === '/api/products/p1').flush(product());
  });

  it('leaves the image alone when an edit picks no file', () => {
    start(product({ images: [uploaded()] }));
    fill('#product-name', 'Moisture Repair Balm');
    submit();

    const patch = http.expectOne((request) => request.url === '/api/products/p1');
    expect(patch.request.body).toEqual({
      name: 'Moisture Repair Balm',
      description: 'Rich barrier cream.',
      price: 67,
      quantity: 4,
      categoryIds: ['c1'],
    });
    patch.flush(product());
  });

  it('will not create a product without an image', () => {
    start();
    fillDetails();
    submit();

    expect(errors()).toContain('Choose an image for this product.');
  });

  it('rejects a file the API would reject anyway', () => {
    start();
    fillDetails();
    pick(new File(['note'], 'notes.txt', { type: 'text/plain' }));

    expect(errors()).toContain('Choose a JPEG, PNG or WebP image.');

    submit();
  });

  it('shows the size limit rather than uploading an oversized file', () => {
    start();
    fillDetails();
    pick(new File([new Uint8Array(6 * 1024 * 1024)], 'huge.png', { type: 'image/png' }));

    expect(errors()).toContain('Image cannot exceed 5 MB.');
  });
});
