import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Observable, catchError, map, of, switchMap, tap } from 'rxjs';
import { describeError, fieldErrors } from '../../../../core/api/api-error';
import {
  Category,
  CreateProductBody,
  IMAGE_ACCEPT,
  MAX_IMAGE_SIZE_IN_MEGABYTES,
  Product,
  imageFileError,
  primaryImage,
  productImageUrl,
} from '../../product';
import { ProductImagesApi } from '../../product-images-api';
import { ProductsApi } from '../../products-api';

interface ProductFormControls {
  readonly name: FormControl<string>;
  readonly description: FormControl<string>;
  readonly price: FormControl<number | null>;
  readonly quantity: FormControl<number | null>;
}

/** Everything except the image path, which is filled in from the upload. */
type ProductFields = Omit<CreateProductBody, 'imagePath'>;

@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductForm implements OnInit {
  private readonly api = inject(ProductsApi);
  private readonly imagesApi = inject(ProductImagesApi);
  private readonly builder = inject(NonNullableFormBuilder);

  /** `null` creates a new product; a product edits it in place. */
  readonly product = input<Product | null>(null);
  readonly categories = input.required<readonly Category[]>();

  readonly saved = output<Product>();
  readonly dismissed = output<void>();

  protected readonly saving = signal(false);
  protected readonly submitted = signal(false);
  protected readonly serverError = signal<string | null>(null);
  protected readonly serverFields = signal<Readonly<Record<string, readonly string[]>>>({});

  protected readonly isEdit = computed(() => this.product() !== null);
  protected readonly title = computed(() => (this.isEdit() ? 'Edit product' : 'New product'));

  protected readonly selectedCategoryIds = signal<readonly string[]>([]);

  protected readonly imageAccept = IMAGE_ACCEPT;
  protected readonly imageHint = `JPEG, PNG or WebP, up to ${MAX_IMAGE_SIZE_IN_MEGABYTES} MB.`;

  /** The file picked from disk, uploaded once the product it belongs to exists. */
  protected readonly imageFile = signal<File | null>(null);
  protected readonly imageError = signal<string | null>(null);

  /** Object URL for the picked file, revoked whenever it is replaced. */
  private readonly previewUrl = signal<string | null>(null);
  /** The image the edited product already has, shown until a file is picked. */
  private readonly storedImageUrl = signal('');

  protected readonly previewSrc = computed(() => this.previewUrl() ?? this.storedImageUrl());

  /**
   * The product this form created, so retrying after a failed upload edits that
   * product instead of creating a second one.
   */
  private readonly created = signal<Product | null>(null);

  protected readonly form: FormGroup<ProductFormControls> = this.builder.group({
    name: this.builder.control('', [
      Validators.required,
      Validators.minLength(5),
      Validators.maxLength(100),
    ]),
    description: this.builder.control('', [
      Validators.required,
      Validators.minLength(5),
      Validators.maxLength(1000),
    ]),
    price: this.builder.control<number | null>(null, [Validators.required, Validators.min(0.01)]),
    quantity: this.builder.control<number | null>(null, [Validators.required, Validators.min(0)]),
  });

  constructor() {
    inject(DestroyRef).onDestroy(() => this.revokePreview());
  }

  // The dialog creates a fresh form each time it opens, so seeding once is enough.
  ngOnInit(): void {
    const product = this.product();
    if (!product) {
      return;
    }

    this.form.setValue({
      name: product.name,
      description: product.description,
      price: product.price,
      quantity: product.quantity,
    });
    this.selectedCategoryIds.set(product.categories.map((category) => category.id));
    this.storedImageUrl.set(productImageUrl(product));
  }

  protected isChecked(categoryId: string): boolean {
    return this.selectedCategoryIds().includes(categoryId);
  }

  protected toggleCategory(categoryId: string, checked: boolean): void {
    this.selectedCategoryIds.update((ids) =>
      checked ? [...new Set([...ids, categoryId])] : ids.filter((id) => id !== categoryId),
    );
  }

  protected onCategoryToggle(categoryId: string, event: Event): void {
    this.toggleCategory(categoryId, (event.target as HTMLInputElement).checked);
  }

  /** Keeps the picked file, or reports why it cannot be uploaded. */
  protected onImageSelected(event: Event): void {
    const picker = event.target as HTMLInputElement;
    const file = picker.files?.[0] ?? null;

    this.revokePreview();
    this.imageFile.set(null);
    this.imageError.set(null);

    if (!file) {
      return;
    }

    const problem = imageFileError(file);
    if (problem) {
      this.imageError.set(problem);
      // Nothing usable is selected any more, and clearing the picker lets the
      // same file be chosen again once it has been converted or resized.
      picker.value = '';
      return;
    }

    this.imageFile.set(file);
    this.previewUrl.set(URL.createObjectURL(file));
  }

  private revokePreview(): void {
    const url = this.previewUrl();
    if (url) {
      URL.revokeObjectURL(url);
    }
    this.previewUrl.set(null);
  }

  private messagesForKey(name: string): readonly string[] {
    const fromServer = this.serverFields();
    const match = Object.keys(fromServer).find((key) => key.toLowerCase() === name.toLowerCase());
    return match ? fromServer[match] : [];
  }

  protected messagesFor(field: keyof ProductFormControls): readonly string[] {
    return this.messagesForKey(field);
  }

  /** Two calls can reject the image: the upload over the file, the product over the path. */
  protected readonly imageMessages = computed<readonly string[]>(() => [
    ...this.messagesForKey('image'),
    ...this.messagesForKey('imagePath'),
  ]);

  protected showError(field: keyof ProductFormControls): boolean {
    const control = this.form.controls[field];
    return (control.touched || this.submitted()) && control.invalid;
  }

  protected cancel(): void {
    this.dismissed.emit();
  }

  protected submit(): void {
    this.submitted.set(true);
    this.serverError.set(null);
    this.serverFields.set({});

    const existing = this.created() ?? this.product();
    const file = this.imageFile();

    // An edit keeps the image it already has; a new product has no fallback.
    if (!file && !existing) {
      this.imageError.set('Choose an image for this product.');
    }

    if (this.form.invalid || this.imageError() !== null || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const fields: ProductFields = {
      name: value.name.trim(),
      description: value.description.trim(),
      price: value.price ?? 0,
      quantity: value.quantity ?? 0,
      categoryIds: this.selectedCategoryIds(),
    };

    if (file) {
      const request = existing
        ? this.saveWithNewImage(existing, fields, file)
        : this.createWithImage(fields, file);
      this.send(request);
      return;
    }

    if (existing) {
      this.send(this.api.update(existing.id, fields));
    }
  }

  private send(request: Observable<Product>): void {
    this.saving.set(true);
    request.subscribe({
      next: (product) => {
        this.saving.set(false);
        this.saved.emit(product);
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.serverError.set(describeError(error));
        this.serverFields.set(fieldErrors(error));
      },
    });
  }

  /**
   * Creating takes three calls: `ImagePath` is required up front, the upload needs
   * a product id that only exists once the row does, and the stored URL is only
   * known once the file is up. The file name stands in until the last call
   * replaces it with the URL the image was stored at.
   */
  private createWithImage(fields: ProductFields, file: File): Observable<Product> {
    return this.api.create({ ...fields, imagePath: file.name }).pipe(
      tap((product) => this.created.set(product)),
      switchMap((product) =>
        this.uploadPrimary(product, file).pipe(
          switchMap((imageUrl) =>
            this.api.update(product.id, { imagePath: imageUrl }).pipe(
              // The product and its image are both stored by now, and `imagePath`
              // is only the fallback for readers that ignore `images` - not worth
              // sending the admin back into the form over.
              catchError(() => of(product)),
            ),
          ),
        ),
      ),
    );
  }

  /** An existing product uploads first, so the new URL rides along with the other edits. */
  private saveWithNewImage(
    product: Product,
    fields: ProductFields,
    file: File,
  ): Observable<Product> {
    return this.uploadPrimary(product, file).pipe(
      switchMap((imageUrl) => this.api.update(product.id, { ...fields, imagePath: imageUrl })),
    );
  }

  /** Stores `file` as the product's primary image and returns the URL it landed on. */
  private uploadPrimary(product: Product, file: File): Observable<string> {
    const current = primaryImage(product);
    const upload = current
      ? this.imagesApi.update(current.id, file, true)
      : this.imagesApi.create({ productId: product.id, file, isPrimary: true, displayOrder: 1 });

    return upload.pipe(map((image) => image.imageUrl));
  }
}
