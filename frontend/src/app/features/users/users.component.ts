import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../core/api/user.service';
import { errorMessage } from '../../core/error-message';
import { roleLabels } from '../../core/labels';
import { Role, User } from '../../core/models';
import { ToastService } from '../../core/toast/toast.service';
import { ModalComponent } from '../../shared/modal.component';

@Component({
  selector: 'app-users',
  imports: [ModalComponent, ReactiveFormsModule],
  templateUrl: './users.component.html',
})
export class UsersComponent {
  private readonly userService = inject(UserService);
  private readonly toast = inject(ToastService);

  protected readonly labels = roleLabels;
  protected readonly roles = Object.keys(roleLabels) as Role[];
  protected readonly users = signal<User[]>([]);
  protected readonly creating = signal(false);
  protected readonly submitted = signal(false);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    username: ['', [Validators.required, Validators.maxLength(50)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(72)]],
    role: ['Operator' as Role, Validators.required],
  });

  constructor() {
    this.load();
  }

  protected openCreate(): void {
    this.form.reset();
    this.submitted.set(false);
    this.formError.set(null);
    this.creating.set(true);
  }

  protected closeForm(): void {
    this.creating.set(false);
  }

  protected submit(): void {
    this.submitted.set(true);
    if (this.form.invalid || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.formError.set(null);

    this.userService.create(this.form.getRawValue()).subscribe({
      next: (user) => {
        this.saving.set(false);
        this.creating.set(false);
        this.toast.success(`Usuário ${user.username} criado.`);
        this.load();
      },
      error: (error) => {
        this.saving.set(false);
        this.formError.set(errorMessage(error));
      },
    });
  }

  private load(): void {
    this.userService.list().subscribe({
      next: (users) => this.users.set(users),
      error: (error) => this.toast.error(errorMessage(error)),
    });
  }
}
