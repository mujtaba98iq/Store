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
  AbstractControl,
  FormControl,
  FormGroup,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { InventoryFormStore } from '../../data-access/inventory-form-store';
import { Inventory } from '../../models/inventory.model';
import { ProductVariant } from '../../models/product-variant.model';

interface InventoryFormControls {
  readonly productVariantId: FormControl<string>;
  readonly quantity: FormControl<number>;
  readonly reservedQuantity: FormControl<number>;
}

/**
 * The invariant `Domain.Inventories.InventoryService` enforces, checked here too so
 * the reader is told before the round trip rather than after it. It belongs on the
 * group because neither box is wrong on its own - only the pair is.
 */
function reservedWithinQuantity(group: AbstractControl): ValidationErrors | null {
  const quantity = group.get('quantity')?.value as number;
  const reserved = group.get('reservedQuantity')?.value as number;

  if (!Number.isFinite(quantity) || !Number.isFinite(reserved)) {
    return null;
  }

  return reserved > quantity ? { reservedTooHigh: true } : null;
}

@Component({
  selector: 'app-inventory-form',
  imports: [ReactiveFormsModule],
  templateUrl: './inventory-form.html',
  styleUrl: './inventory-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [InventoryFormStore],
})
export class InventoryForm implements OnInit {
  private readonly store = inject(InventoryFormStore);
  private readonly builder = inject(NonNullableFormBuilder);

  /** `null` opens a new stock row; an inventory edits that one in place. */
  readonly inventory = input<Inventory | null>(null);

  /** Every variant the shop has, for the picker a new row starts from. */
  readonly variants = input.required<readonly ProductVariant[]>();

  /** Held open while the lookup is in flight, so the picker can say why it is empty. */
  readonly variantsLoading = input(false);

  readonly saved = output<Inventory>();
  readonly dismissed = output<void>();

  protected readonly saving = this.store.saving;

  protected readonly submitted = signal(false);

  protected readonly isEdit = computed(() => this.inventory() !== null);
  protected readonly title = computed(() => (this.isEdit() ? 'Edit stock' : 'New stock row'));

  /**
   * On an edit the variant is settled, so the row is labelled by its SKU instead of
   * offering a picker the API would ignore.
   */
  protected readonly editingSku = computed(() => this.inventory()?.sku ?? 'this variant');

  protected readonly quantityHint = 'Units on hand, reserved ones included.';
  protected readonly reservedHint = 'Units already promised to an order. Never more than on hand.';
  protected readonly variantHint =
    'A variant can hold one stock row. One that already has some is refused.';

  protected readonly form: FormGroup<InventoryFormControls> = this.builder.group(
    {
      // Relaxed in ngOnInit for an edit, where the control is not rendered at all.
      productVariantId: this.builder.control('', [Validators.required]),
      quantity: this.builder.control(0, [Validators.required, Validators.min(0)]),
      reservedQuantity: this.builder.control(0, [Validators.required, Validators.min(0)]),
    },
    { validators: reservedWithinQuantity },
  );

  private readonly value = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });

  /**
   * What the API will compute for this row, shown as the admin types - the number
   * they actually care about is the one neither box holds.
   */
  protected readonly availablePreview = computed(() => {
    const { quantity, reservedQuantity } = this.value();
    return Math.max(0, (Number(quantity) || 0) - (Number(reservedQuantity) || 0));
  });

  // The dialog creates a fresh form each time it opens, so seeding once is enough.
  ngOnInit(): void {
    const inventory = this.inventory();
    if (!inventory) {
      return;
    }

    // The variant is fixed for the row's lifetime and the picker is not rendered,
    // so a required control here would only hold the form invalid forever.
    this.form.controls.productVariantId.clearValidators();
    this.form.controls.productVariantId.updateValueAndValidity();

    this.form.setValue({
      productVariantId: inventory.productVariantId,
      quantity: inventory.quantity,
      reservedQuantity: inventory.reservedQuantity,
    });
  }

  protected messagesFor(field: keyof InventoryFormControls): readonly string[] {
    const fromServer = this.store.serverFields();
    const match = Object.keys(fromServer).find((key) => key.toLowerCase() === field.toLowerCase());
    return match ? fromServer[match] : [];
  }

  protected showError(field: keyof InventoryFormControls): boolean {
    const control = this.form.controls[field];
    return (control.touched || this.submitted()) && control.invalid;
  }

  /** The cross-field failure, which belongs to the pair rather than to one box. */
  protected showReservedTooHigh(): boolean {
    const touched = this.form.controls.reservedQuantity.touched || this.submitted();
    return touched && this.form.hasError('reservedTooHigh');
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

    this.store
      .save(this.inventory(), {
        productVariantId: value.productVariantId,
        quantity: Number(value.quantity),
        reservedQuantity: Number(value.reservedQuantity),
      })
      .subscribe((inventory) => this.saved.emit(inventory));
  }
}
