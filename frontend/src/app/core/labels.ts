import { MovementType, Role } from './models';

export const roleLabels: Record<Role, string> = {
  Administrator: 'Administrador',
  Operator: 'Operador',
  Consultant: 'Consultor',
};

export const movementLabels: Record<MovementType, string> = {
  StockIn: 'Entrada',
  StockOut: 'Saída',
  Reservation: 'Reserva',
  ReservationCancellation: 'Cancelamento de reserva',
  ReservationFulfillment: 'Atendimento de reserva',
  Adjustment: 'Ajuste manual',
};
