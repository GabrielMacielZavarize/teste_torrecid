import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { errorMessage } from '../../core/error-message';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly form = inject(NonNullableFormBuilder).group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });
  protected readonly sessionExpired = inject(ActivatedRoute).snapshot.queryParamMap.has('expired');
  protected readonly submitted = signal(false);
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected submit(): void {
    this.submitted.set(true);
    if (this.form.invalid || this.submitting()) {
      return;
    }

    const { username, password } = this.form.getRawValue();
    this.submitting.set(true);
    this.error.set(null);

    this.auth.login(username, password).subscribe({
      next: () => this.router.navigate(['/']),
      error: (error) => {
        this.error.set(errorMessage(error));
        this.submitting.set(false);
      },
    });
  }
}
