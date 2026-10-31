using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Services;

// Правило неявки: если автомобиль не приехал в течение 15 минут после
// назначенного времени, бронь снимается и место возвращается в свободные.
public static class BookingRules
{
    public static TimeSpan NoShowDelay => TimeSpan.FromMinutes(15);

    public static DateTime HeldUntil(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        return booking.ArrivalTime + NoShowDelay;
    }

    // Снимает просроченные брони; возвращает их количество.
    public static int ExpireNoShows(IBookingRepository bookings, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(bookings);

        var expired = bookings.GetWaiting().Where(b => now > HeldUntil(b)).ToList();
        foreach (var booking in expired)
        {
            booking.Status = BookingStatus.NoShow;
            bookings.Update(booking);
        }

        return expired.Count;
    }
}
