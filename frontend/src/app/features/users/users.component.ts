import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../core/api/user.service';
import { errorMessage } from '../../core/error-message';
import { roleLabels } from '../../core/labels';
import { Role, User } from '../../core/models';

@Component({
  selector: 'app-users',
  imports: [ReactiveFormsModule],
  templateUrl: './users.component.html',
})
export class UsersComponent {
  private readonly userService = inject(UserService);

  protected readonly labels = roleLabels;
  protected readonly roles = Object.keys(roleLabels) as Role[];
  protected readonly users = signal<User[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly submitted = signal(false);
  protected readonly saving = signal(false);

  protected readonly form = inject(NonNullableFormBuilder).group({
    username: ['', [Validators.required, Validators.maxLength(50)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(72)]],
    role: ['Operator' as Role, Validators.required],
  });

  constructor() {
    this.load();
  }

  protected submit(): void {
    this.submitted.set(true);
    if (this.form.invalid || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.success.set(null);

    this.userService.create(this.form.getRawValue()).subscribe({
      next: (user) => {
        this.saving.set(false);
        this.submitted.set(false);
        this.form.reset();
        this.success.set(`Usuário ${user.username} criado.`);
        this.load();
      },
      error: (error) => {
        this.saving.set(false);
        this.error.set(errorMessage(error));
      },
    });
  }

  private load(): void {
    this.userService.list().subscribe({
      next: (users) => this.users.set(users),
      error: (error) => this.error.set(errorMessage(error)),
    });
  }
}
