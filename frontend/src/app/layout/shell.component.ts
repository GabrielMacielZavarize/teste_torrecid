import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css',
})
export class ShellComponent {
  private readonly router = inject(Router);
  protected readonly auth = inject(AuthService);

  protected readonly username = computed(() => this.auth.session()?.username ?? '');

  protected logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
