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
import { CategoryDetail } from '@app/core/models/interfaces/category';
import { CategoryFormStore } from '../../data-access/category-form-store';

interface CategoryFormControls {
  readonly name: FormControl<string>;
  readonly description: FormControl<string>;
}

/** Matches `RestApi.Categories.CreateCategoryRequestValidator`. */
const NAME_MIN = 3;
const NAME_MAX = 100;
const DESCRIPTION_MIN = 3;
const DESCRIPTION_MAX = 1000;

@Component({
  selector: 'app-category-form',
  imports: [ReactiveFormsModule],
  templateUrl: './category-form.html',
  styleUrl: './category-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [CategoryFormStore],
})
export class CategoryForm implements OnInit {
  private readonly store = inject(CategoryFormStore);
  private readonly builder = inject(NonNullableFormBuilder);

  /** `null` creates a new category; a category edits it in place. */
  readonly category = input<CategoryDetail | null>(null);

  readonly saved = output<CategoryDetail>();
  readonly dismissed = output<void>();

  protected readonly saving = this.store.saving;
  protected readonly serverError = this.store.serverError;

  protected readonly submitted = signal(false);

  protected readonly isEdit = computed(() => this.category() !== null);
  protected readonly title = computed(() => (this.isEdit() ? 'Edit category' : 'New category'));

  protected readonly nameHint = `Between ${NAME_MIN} and ${NAME_MAX} characters.`;
  protected readonly descriptionHint = `Optional, but ${DESCRIPTION_MIN} characters or more when given.`;

  protected readonly form: FormGroup<CategoryFormControls> = this.builder.group({
    name: this.builder.control('', [
      Validators.required,
      Validators.minLength(NAME_MIN),
      Validators.maxLength(NAME_MAX),
    ]),
    // Optional, so the length floor only applies once something is typed.
    description: this.builder.control('', [
      Validators.minLength(DESCRIPTION_MIN),
      Validators.maxLength(DESCRIPTION_MAX),
    ]),
  });

  // The dialog creates a fresh form each time it opens, so seeding once is enough.
  ngOnInit(): void {
    const category = this.category();
    if (!category) {
      return;
    }

    this.form.setValue({
      name: category.name,
      description: category.description ?? '',
    });
  }

  protected messagesFor(field: keyof CategoryFormControls): readonly string[] {
    const fromServer = this.store.serverFields();
    const match = Object.keys(fromServer).find((key) => key.toLowerCase() === field.toLowerCase());
    return match ? fromServer[match] : [];
  }

  protected showError(field: keyof CategoryFormControls): boolean {
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
    const description = value.description.trim();

    this.store
      .save(this.category(), {
        name: value.name.trim(),
        // The column is nullable; an emptied box should clear it, not store "".
        description: description || null,
      })
      .subscribe((category) => this.saved.emit(category));
  }
}
