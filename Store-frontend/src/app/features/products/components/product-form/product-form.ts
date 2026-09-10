import {
  ChangeDetectionStrategy,
  Component,
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
import { describeError, fieldErrors } from '../../../../core/api/api-error';
import { Category, CreateProductBody, Product } from '../../product';
import { ProductsApi } from '../../products-api';

interface ProductFormControls {
  readonly name: FormControl<string>;
  readonly description: FormControl<string>;
  readonly price: FormControl<number | null>;
  readonly quantity: FormControl<number | null>;
  readonly imagePath: FormControl<string>;
}

@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductForm implements OnInit {
  private readonly api = inject(ProductsApi);
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
    imagePath: this.builder.control('', [Validators.required, Validators.maxLength(500)]),
  });

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
      imagePath: product.imagePath,
    });
    this.selectedCategoryIds.set(product.categories.map((category) => category.id));
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

  protected messagesFor(field: keyof ProductFormControls): readonly string[] {
    const fromServer = this.serverFields();
    const match = Object.keys(fromServer).find((key) => key.toLowerCase() === field.toLowerCase());
    return match ? fromServer[match] : [];
  }

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

    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const body: CreateProductBody = {
      name: value.name.trim(),
      description: value.description.trim(),
      price: value.price ?? 0,
      quantity: value.quantity ?? 0,
      imagePath: value.imagePath.trim(),
      categoryIds: this.selectedCategoryIds(),
    };

    const existing = this.product();
    const request = existing ? this.api.update(existing.id, body) : this.api.create(body);

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
}
