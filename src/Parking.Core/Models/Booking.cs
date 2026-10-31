namespace Parking.Core.Models;

// Бронь места категории на время прибытия (на текущий день).
// Конкретное место не закрепляется: бронь удерживает одно место своей категории.
public class Booking
{
    public int Id { get; set; }

    public string CarNumber { get; set; } = string.Empty;

    public SpotCategory Category { get; set; }

    public DateTime ArrivalTime { get; set; }

    public DateTime CreatedTime { get; set; }

    public BookingStatus Status { get; set; }

    // Стоянка, начатая по этой брони.
    public int? SessionId { get; set; }
}
