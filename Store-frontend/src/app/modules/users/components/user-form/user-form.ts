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
import { UserFormStore } from '../../data-access/user-form-store';
import { User, UserRole } from '../../models/user.model';

interface UserFormControls {
  readonly username: FormControl<string>;
  readonly password: FormControl<string>;
  readonly role: FormControl<string>;
}

/** Matches `RestApi.Users.CreateUserRequestValidator`. */
const USERNAME_MIN = 3;
const PASSWORD_MIN = 6;

/** The roles the API recognises, in the order the picker offers them. */
const ROLES: readonly { readonly label: string; readonly value: string }[] = [
  { label: 'Customer', value: UserRole.User },
  { label: 'Admin', value: UserRole.Admin },
];

@Component({
  selector: 'app-user-form',
  imports: [ReactiveFormsModule],
  templateUrl: './user-form.html',
  styleUrl: './user-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [UserFormStore],
})
export class UserForm implements OnInit {
  private readonly store = inject(UserFormStore);
  private readonly builder = inject(NonNullableFormBuilder);

  /** `null` creates a new account; a user edits that one in place. */
  readonly user = input<User | null>(null);

  readonly saved = output<User>();
  readonly dismissed = output<void>();

  protected readonly roles = ROLES;
  protected readonly saving = this.store.saving;

  protected readonly submitted = signal(false);

  protected readonly isEdit = computed(() => this.user() !== null);
  protected readonly title = computed(() => (this.isEdit() ? 'Edit user' : 'New user'));

  protected readonly usernameHint = `At least ${USERNAME_MIN} characters, and not one another account already holds.`;
  protected readonly passwordHint = computed(() =>
    this.isEdit()
      ? 'Leave blank to keep the current password.'
      : `At least ${PASSWORD_MIN} characters.`,
  );

  protected readonly form: FormGroup<UserFormControls> = this.builder.group({
    username: this.builder.control('', [Validators.required, Validators.minLength(USERNAME_MIN)]),
    // Required only when creating - see ngOnInit, which relaxes it for an edit.
    password: this.builder.control('', [Validators.required, Validators.minLength(PASSWORD_MIN)]),
    role: this.builder.control<string>(UserRole.User, [Validators.required]),
  });

  // The dialog creates a fresh form each time it opens, so seeding once is enough.
  ngOnInit(): void {
    const user = this.user();
    if (!user) {
      return;
    }

    // Editing never shows the existing password - the API only ever sends the hash,
    // and never that - so the box stands for "set a new one", and may stay empty.
    this.form.controls.password.setValidators([Validators.minLength(PASSWORD_MIN)]);
    this.form.controls.password.updateValueAndValidity();

    this.form.setValue({
      username: user.username,
      password: '',
      role: user.role,
    });
  }

  protected messagesFor(field: keyof UserFormControls): readonly string[] {
    const fromServer = this.store.serverFields();
    const match = Object.keys(fromServer).find((key) => key.toLowerCase() === field.toLowerCase());
    return match ? fromServer[match] : [];
  }

  protected showError(field: keyof UserFormControls): boolean {
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

    this.store
      .save(this.user(), {
        username: value.username.trim(),
        password: value.password,
        role: value.role,
      })
      .subscribe((user) => this.saved.emit(user));
  }
}
