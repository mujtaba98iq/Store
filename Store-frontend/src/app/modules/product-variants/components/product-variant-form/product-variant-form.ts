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
import { ProductVariantFormStore } from '../../data-access/product-variant-form-store';
import { CreateProductVariantBody, ProductVariant } from '../../models/product-variant.model';
import { ProductOption } from '../../models/product.model';

interface ProductVariantFormControls {
  readonly productId: FormControl<string>;
  readonly sku: FormControl<string>;
  /**
   * Null when the box is empty, which is a real answer here: the variant then
   * sells at the product’s price. `NumberValueAccessor` is what fills this control
   * - with a number, or with null - so it is typed the way the DOM actually fills it.
   */
  readonly price: FormControl<number | null>;
  readonly barcode: FormControl<string>;
  readonly isActive: FormControl<boolean>;
}

/** What `CreateProductVariantRequestValidator` enforces, so the reader hears it first. */
const SKU_MIN_LENGTH = 3;
const SKU_MAX_LENGTH = 50;
const BARCODE_MIN_LENGTH = 6;
const BARCODE_MAX_LENGTH = 50;

/**
 * A blank box means "no price", not zero - and a half-typed "1e" arrives as NaN,
 * which is not a price either. Anything that is not a number above zero is treated
 * as not given, and left off the request entirely.
 */
function toPrice(raw: number | null): number | undefined {
  return raw !== null && Number.isFinite(raw) && raw > 0 ? raw : undefined;
}

@Component({
  selector: 'app-product-variant-form',
  imports: [ReactiveFormsModule],
  templateUrl: './product-variant-form.html',
  styleUrl: './product-variant-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ProductVariantFormStore],
})
export class ProductVariantForm implements OnInit {
  private readonly store = inject(ProductVariantFormStore);
  private readonly builder = inject(NonNullableFormBuilder);

  /** `null` opens a new variant; a variant edits that one in place. */
  readonly variant = input<ProductVariant | null>(null);

  /** Every product the shop has, for the picker a new variant starts from. */
  readonly products = input.required<readonly ProductOption[]>();

  /** Held open while the lookup is in flight, so the picker can say why it is empty. */
  readonly productsLoading = input(false);

  readonly saved = output<ProductVariant>();
  readonly dismissed = output<void>();

  protected readonly saving = this.store.saving;

  protected readonly submitted = signal(false);

  protected readonly isEdit = computed(() => this.variant() !== null);
  protected readonly title = computed(() => (this.isEdit() ? 'Edit variant' : 'New variant'));

  /**
   * On an edit the product is settled, so the row is labelled by its name instead of
   * offering a picker the API would ignore.
   */
  protected readonly editingProduct = computed(() => this.variant()?.productName ?? 'this product');

  protected readonly skuHint = 'Unique across the shop. One that another variant holds is refused.';
  protected readonly productHint = 'The product this variant is a form of - a size, a shade.';
  protected readonly activeHint = 'An inactive variant stays on the books but cannot be bought.';

  /**
   * The API reads a missing field and a null one the same way, so an edit cannot
   * empty either box once it holds something. Saying so beats letting the admin
   * clear it and watch the old value come back.
   */
  protected readonly priceHint = computed(() =>
    this.isEdit()
      ? 'Leave blank to sell at the product’s price. Clearing a price already set is not possible yet.'
      : 'Optional. Leave blank to sell at the product’s price.',
  );

  protected readonly barcodeHint = computed(() =>
    this.isEdit()
      ? 'Clearing a barcode already set is not possible yet.'
      : `Optional. ${BARCODE_MIN_LENGTH}–${BARCODE_MAX_LENGTH} characters.`,
  );

  protected readonly form: FormGroup<ProductVariantFormControls> = this.builder.group({
    // Relaxed in ngOnInit for an edit, where the control is not rendered at all.
    productId: this.builder.control('', [Validators.required]),
    sku: this.builder.control('', [
      Validators.required,
      Validators.minLength(SKU_MIN_LENGTH),
      Validators.maxLength(SKU_MAX_LENGTH),
    ]),
    price: this.builder.control<number | null>(null, [Validators.min(0.01)]),
    barcode: this.builder.control('', [
      Validators.minLength(BARCODE_MIN_LENGTH),
      Validators.maxLength(BARCODE_MAX_LENGTH),
    ]),
    isActive: this.builder.control(true),
  });

  // The dialog creates a fresh form each time it opens, so seeding once is enough.
  ngOnInit(): void {
    const variant = this.variant();
    if (!variant) {
      return;
    }

    // The product is fixed for the variant's lifetime and the picker is not
    // rendered, so a required control here would only hold the form invalid forever.
    this.form.controls.productId.clearValidators();
    this.form.controls.productId.updateValueAndValidity();

    this.form.setValue({
      productId: variant.productId,
      sku: variant.sku,
      price: variant.price,
      barcode: variant.barcode ?? '',
      isActive: variant.isActive,
    });
  }

  protected messagesFor(field: keyof ProductVariantFormControls): readonly string[] {
    const fromServer = this.store.serverFields();
    const match = Object.keys(fromServer).find((key) => key.toLowerCase() === field.toLowerCase());
    return match ? fromServer[match] : [];
  }

  protected showError(field: keyof ProductVariantFormControls): boolean {
    const control = this.form.controls[field];
    return (control.touched || this.submitted()) && control.invalid;
  }

  protected cancel(): void {
    this.dismissed.emit();
  }

  protected submit(): void {
    this.submitted.set(true);

    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const price = toPrice(value.price);
    const barcode = value.barcode.trim();

    const body: CreateProductVariantBody = {
      productId: value.productId,
      sku: value.sku.trim(),
      isActive: value.isActive,
      // Left off rather than sent empty: the API takes a missing price as "no price
      // of its own", and an empty barcode is not a barcode.
      ...(price !== undefined ? { price } : {}),
      ...(barcode ? { barcode } : {}),
    };

    this.store.save(this.variant(), body).subscribe((variant) => this.saved.emit(variant));
  }
}
