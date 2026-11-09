import { Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-container',
  template: `
    <div class="toasts">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          class="toast"
          [class.toast-error]="toast.kind === 'error'"
          [attr.role]="toast.kind === 'error' ? 'alert' : 'status'"
        >
          <span>{{ toast.message }}</span>
          <button type="button" aria-label="Fechar aviso" (click)="toastService.dismiss(toast.id)">
            &times;
          </button>
        </div>
      }
    </div>
  `,
  styles: `
    .toasts {
      position: fixed;
      bottom: 16px;
      right: 16px;
      z-index: 100;
      display: flex;
      flex-direction: column;
      gap: 8px;
      width: min(360px, calc(100vw - 32px));
      pointer-events: none;
    }

    .toast {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 12px;
      padding: 10px 12px 10px 14px;
      border-left: 4px solid var(--accent);
      border-radius: var(--radius);
      background: var(--surface);
      box-shadow: 0 6px 20px rgb(29 33 37 / 22%);
      pointer-events: auto;
      animation: toast-in 180ms ease-out;
    }

    .toast-error {
      border-left-color: var(--danger);
    }

    button {
      padding: 0 4px;
      border: 0;
      background: transparent;
      color: var(--muted);
      font-size: 1.2rem;
      line-height: 1.2;
      cursor: pointer;
    }

    @keyframes toast-in {
      from {
        opacity: 0;
        transform: translateX(16px);
      }
    }

    @media (prefers-reduced-motion: reduce) {
      .toast {
        animation: none;
      }
    }
  `,
})
export class ToastContainerComponent {
  protected readonly toastService = inject(ToastService);
}
