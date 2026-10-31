using Parking.Core.Models;

namespace Parking.Core.Services;

public interface IBookingService
{
    // Брони на день (по времени прибытия); просроченные перед этим снимаются.
    IReadOnlyList<Booking> GetBookingsForDay(DateTime day);

    Booking CreateBooking(string carNumber, SpotCategory category, DateTime arrivalTime);

    void CancelBooking(int bookingId);
}
