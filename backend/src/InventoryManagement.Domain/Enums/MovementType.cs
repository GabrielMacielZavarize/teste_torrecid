namespace InventoryManagement.Domain.Enums;

public enum MovementType
{
    StockIn = 1,
    StockOut = 2,
    Reservation = 3,
    ReservationCancellation = 4,
    ReservationFulfillment = 5,
    Adjustment = 6
}