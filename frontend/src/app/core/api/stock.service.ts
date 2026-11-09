import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResult, Reservation, StockMovement } from '../models';

export interface StockOperationInput {
  quantity: number;
  reason: string | null;
}

@Injectable({ providedIn: 'root' })
export class StockService {
  private readonly http = inject(HttpClient);

  stockIn(productId: string, input: StockOperationInput): Observable<StockMovement> {
    return this.http.post<StockMovement>(`/api/products/${productId}/stock/in`, input);
  }

  stockOut(productId: string, input: StockOperationInput): Observable<StockMovement> {
    return this.http.post<StockMovement>(`/api/products/${productId}/stock/out`, input);
  }

  adjust(productId: string, countedQuantity: number, reason: string): Observable<StockMovement> {
    return this.http.post<StockMovement>(`/api/products/${productId}/stock/adjust`, {
      countedQuantity,
      reason,
    });
  }

  reserve(productId: string, input: StockOperationInput): Observable<Reservation> {
    return this.http.post<Reservation>(`/api/products/${productId}/reservations`, input);
  }

  listActiveReservations(productId: string): Observable<Reservation[]> {
    const params = new HttpParams().set('productId', productId).set('status', 'Active');
    return this.http.get<Reservation[]>('/api/reservations', { params });
  }

  cancelReservation(reservationId: string): Observable<StockMovement> {
    return this.http.post<StockMovement>(`/api/reservations/${reservationId}/cancel`, {});
  }

  fulfillReservation(reservationId: string): Observable<StockMovement> {
    return this.http.post<StockMovement>(`/api/reservations/${reservationId}/fulfill`, {});
  }

  history(
    productId: string | null,
    page: number,
    pageSize: number,
  ): Observable<PagedResult<StockMovement>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (productId) {
      params = params.set('productId', productId);
    }

    return this.http.get<PagedResult<StockMovement>>('/api/stock/movements', { params });
  }
}
