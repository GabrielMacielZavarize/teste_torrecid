import {
  AfterViewInit,
  Component,
  ElementRef,
  input,
  OnDestroy,
  output,
  viewChild,
} from '@angular/core';

@Component({
  selector: 'app-modal',
  template: `
    <dialog #dialog aria-labelledby="modal-title" (close)="closed.emit()">
      <header>
        <h2 id="modal-title">{{ title() }}</h2>
        <button type="button" aria-label="Fechar" (click)="closed.emit()">&times;</button>
      </header>
      <div class="modal-body">
        <ng-content />
      </div>
    </dialog>
  `,
  styles: `
    dialog {
      width: min(480px, calc(100vw - 32px));
      padding: 0;
      border: 0;
      border-radius: 10px;
      color: var(--ink);
      box-shadow: 0 16px 48px rgb(29 33 37 / 30%);
    }

    dialog::backdrop {
      background: rgb(29 33 37 / 50%);
    }

    header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 14px 20px;
      border-bottom: 1px solid var(--line);
    }

    header button {
      padding: 0 4px;
      border: 0;
      background: transparent;
      color: var(--muted);
      font-size: 1.5rem;
      line-height: 1;
      cursor: pointer;
    }

    .modal-body {
      padding: 16px 20px 20px;
    }
  `,
})
export class ModalComponent implements AfterViewInit, OnDestroy {
  readonly title = input.required<string>();
  readonly closed = output<void>();

  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  private previousFocus: HTMLElement | null = null;

  ngAfterViewInit(): void {
    this.previousFocus = document.activeElement as HTMLElement | null;
    this.dialog().nativeElement.showModal();
  }

  ngOnDestroy(): void {
    this.previousFocus?.focus();
  }
}
