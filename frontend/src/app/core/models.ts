export type Role = 'Administrator' | 'Operator' | 'Consultant';

export type Permission =
  | 'Products.View'
  | 'Products.Manage'
  | 'Stock.Move'
  | 'Stock.ViewHistory'
  | 'Users.Manage';

export interface Session {
  token: string;
  expiresAt: string;
  username: string;
  role: Role;
  permissions: Permission[];
}

export interface Product {
  id: string;
  sku: string;
  name: string;
  description: string | null;
  isActive: boolean;
  physicalQuantity: number;
  reservedQuantity: number;
  availableQuantity: number;
}

export type MovementType =
  | 'StockIn'
  | 'StockOut'
  | 'Reservation'
  | 'ReservationCancellation'
  | 'ReservationFulfillment'
  | 'Adjustment';

export interface StockMovement {
  id: string;
  productId: string;
  productSku: string;
  productName: string;
  reservationId: string | null;
  type: MovementType;
  quantity: number;
  physicalBefore: number;
  reservedBefore: number;
  physicalAfter: number;
  reservedAfter: number;
  userId: string;
  username: string;
  occurredAt: string;
  reason: string | null;
}

export type ReservationStatus = 'Active' | 'Cancelled' | 'Fulfilled';

export interface Reservation {
  id: string;
  productId: string;
  quantity: number;
  status: ReservationStatus;
  createdByUserId: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface User {
  id: string;
  username: string;
  role: Role;
}
