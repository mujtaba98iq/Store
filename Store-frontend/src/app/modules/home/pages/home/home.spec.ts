import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { Home } from './home';

describe('Home', () => {
  let fixture: ComponentFixture<Home>;
  let element: HTMLElement;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Home);
    element = fixture.nativeElement as HTMLElement;
  });

  afterEach(() => {
    http.verify({ ignoreCancelled: true });
  });

  it('asks the API for the single newest product', () => {
    fixture.detectChanges();

    const request = http.expectOne((candidate) => candidate.url === '/api/products');
    expect(request.request.params.get('PageSize')).toBe('1');
    expect(request.request.params.get('OrderBy')).toBe('1');
    expect(request.request.params.get('OrderByDirection')).toBe('1');
    request.flush({ totalCount: 0, data: [] });
  });

  /** Answers the hero's read with one product and renders the card. */
  const showFeatured = async () => {
    fixture.detectChanges();
    http
      .expectOne((candidate) => candidate.url === '/api/products')
      .flush({
        totalCount: 1,
        data: [
          {
            id: 'p1',
            name: 'Radiance Renewal Serum',
            description: 'A daily vitamin C concentrate.',
            price: 84,
            quantity: 18,
            imagePath: '/product-images/product-serum.svg',
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: null,
            categories: [{ id: 'c1', name: 'Serums', description: null }],
            variants: [],
            images: [
              {
                id: 'i1',
                productId: 'p1',
                imageUrl: 'https://res.cloudinary.com/lvkdjyco/image/upload/v1/store/serum.webp',
                isPrimary: true,
                displayOrder: 1,
              },
            ],
          },
        ],
      });
    await Promise.resolve();
    fixture.detectChanges();
  };

  it('shows the returned product in the hero card', async () => {
    await showFeatured();

    expect(element.querySelector('.card__name')?.textContent?.trim()).toBe(
      'Radiance Renewal Serum',
    );
    expect(element.querySelector('.card__media img')?.getAttribute('src')).toContain(
      'res.cloudinary.com/lvkdjyco/image/upload/v1/store/serum.webp',
    );
  });

  it('opens the featured product rather than carting it, which needs a variant first', async () => {
    await showFeatured();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    const add = Array.from(element.querySelectorAll<HTMLButtonElement>('button')).find(
      (button) => button.textContent?.trim() === 'Choose options',
    );
    expect(add).toBeDefined();

    add!.click();

    expect(navigate).toHaveBeenCalledWith(['/products', 'p1']);
  });

  it('renders the hero without a card when the catalogue is empty', async () => {
    fixture.detectChanges();
    http
      .expectOne((candidate) => candidate.url === '/api/products')
      .flush({ totalCount: 0, data: [] });
    await Promise.resolve();
    fixture.detectChanges();

    expect(element.querySelector('.hero__title')).not.toBeNull();
    expect(element.querySelector('app-product-card')).toBeNull();
  });
});
