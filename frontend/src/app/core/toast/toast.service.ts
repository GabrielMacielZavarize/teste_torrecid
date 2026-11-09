import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  kind: 'success' | 'error';
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  private readonly items = signal<Toast[]>([]);

  readonly toasts = this.items.asReadonly();

  success(message: string): void {
    this.show('success', message, 4000);
  }

  error(message: string): void {
    this.show('error', message, 7000);
  }

  dismiss(id: number): void {
    this.items.update((toasts) => toasts.filter((toast) => toast.id !== id));
  }

  private show(kind: Toast['kind'], message: string, duration: number): void {
    const id = ++this.nextId;
    this.items.update((toasts) => [...toasts, { id, kind, message }]);
    setTimeout(() => this.dismiss(id), duration);
  }
}
