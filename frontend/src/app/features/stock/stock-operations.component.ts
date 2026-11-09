import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { ProductService } from '../../core/api/product.service';
import { StockService } from '../../core/api/stock.service';
import { errorMessage } from '../../core/error-message';
import { Product, Reservation } from '../../core/models';

type Operation = 'in' | 'out' | 'reserve' | 'adjust';

interface OperationOption {
  id: Operation;
  label: string;
  quantityLabel: string;
  successMessage: string;
}

const integerPattern = /^-?\d+$/;

@Component({
  selector: 'app-stock-operations',
  imports: [DatePipe, ReactiveFormsModule, RouterLink],
  templateUrl: './stock-operations.component.html',
  styleUrl: './stock-operations.component.css',
})
export class StockOperationsComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly stockService = inject(StockService);

  // Preenchido pelo parâmetro :id da rota.
  readonly id = input.required<string>();

  protected readonly operations: OperationOption[] = [
    { id: 'in', label: 'Entrada', quantityLabel: 'Quantidade', successMessage: 'Entrada registrada.' },
    { id: 'out', label: 'Saída', quantityLabel: 'Quantidade', successMessage: 'Saída registrada.' },
    { id: 'reserve', label: 'Reserva', quantityLabel: 'Quantidade', successMessage: 'Reserva criada.' },
    { id: 'adjust', label: 'Ajuste manual', quantityLabel: 'Quantidade contada', successMessage: 'Ajuste registrado.' },
  ];

  protected readonly product = signal<Product | null>(null);
  protected readonly reservations = signal<Reservation[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly submitted = signal(false);
  protected readonly saving = signal(false);
  protected readonly busyReservationId = signal<string | null>(null);

  protected readonly operation = signal<Operation>('in');
  protected readonly selected = computed(
    () => this.operations.find((option) => option.id === this.operation())!,
  );

  protected readonly form = new FormGroup({
    quantity: new FormControl<number | null>(null),
    reason: new FormControl('', { nonNullable: true, validators: Validators.maxLength(500) }),
  });

  ngOnInit(): void {
    this.applyValidators();
    this.refresh();
  }

  protected select(operation: Operation): void {
    this.operation.set(operation);
    this.submitted.set(false);
    this.success.set(null);
    this.error.set(null);
    this.applyValidators();
  }

  protected submit(): void {
    this.submitted.set(true);
    const quantity = this.form.controls.quantity.value;
    if (this.form.invalid || this.saving() || quantity === null) {
      return;
    }

    const option = this.selected();
    const reason = this.form.controls.reason.value.trim();

    this.saving.set(true);
    this.error.set(null);
    this.success.set(null);

    this.requestFor(option.id, quantity, reason).subscribe({
      next: () => {
        this.saving.set(false);
        this.submitted.set(false);
        this.form.reset();
        this.success.set(option.successMessage);
        this.refresh();
      },
      error: (error) => {
        this.saving.set(false);
        this.error.set(errorMessage(error));
        // O saldo pode ter mudado por outra operação; mostra o valor atual.
        this.refresh();
      },
    });
  }

  protected cancelReservation(reservation: Reservation): void {
    this.runReservationAction(
      reservation,
      this.stockService.cancelReservation(reservation.id),
      'Reserva cancelada.',
    );
  }

  protected fulfillReservation(reservation: Reservation): void {
    this.runReservationAction(
      reservation,
      this.stockService.fulfillReservation(reservation.id),
      'Reserva atendida.',
    );
  }

  private requestFor(operation: Operation, quantity: number, reason: string): Observable<unknown> {
    const productId = this.id();
    const input = { quantity, reason: reason || null };

    switch (operation) {
      case 'in':
        return this.stockService.stockIn(productId, input);
      case 'out':
        return this.stockService.stockOut(productId, input);
      case 'reserve':
        return this.stockService.reserve(productId, input);
      case 'adjust':
        return this.stockService.adjust(productId, quantity, reason);
    }
  }

  private runReservationAction(
    reservation: Reservation,
    request: Observable<unknown>,
    successMessage: string,
  ): void {
    this.busyReservationId.set(reservation.id);
    this.error.set(null);
    this.success.set(null);

    request.subscribe({
      next: () => {
        this.busyReservationId.set(null);
        this.success.set(successMessage);
        this.refresh();
      },
      error: (error) => {
        this.busyReservationId.set(null);
        this.error.set(errorMessage(error));
        this.refresh();
      },
    });
  }

  private applyValidators(): void {
    const isAdjustment = this.operation() === 'adjust';
    const { quantity, reason } = this.form.controls;

    quantity.setValidators([
      Validators.required,
      Validators.min(isAdjustment ? 0 : 1),
      Validators.pattern(integerPattern),
    ]);
    reason.setValidators(
      isAdjustment ? [Validators.required, Validators.maxLength(500)] : Validators.maxLength(500),
    );
    quantity.updateValueAndValidity();
    reason.updateValueAndValidity();
  }

  private refresh(): void {
    const productId = this.id();

    this.productService.getById(productId).subscribe({
      next: (product) => this.product.set(product),
      error: (error) => this.error.set(errorMessage(error)),
    });
    this.stockService.listActiveReservations(productId).subscribe({
      next: (reservations) => this.reservations.set(reservations),
      error: (error) => this.error.set(errorMessage(error)),
    });
  }
}
