using System.Globalization;
using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Services;

// Бронирование мест. Правила:
// бронь делается на текущий день и на время не раньше текущего;
// у автомобиля одна активная бронь, и он не должен уже стоять на парковке;
// бронь удерживает свободное место категории до времени прибытия + 15 минут.
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;
    private readonly IParkingService _parking;
    private readonly TimeProvider _timeProvider;

    public BookingService(IBookingRepository bookings, IParkingService parking, TimeProvider timeProvider)
    {
        _bookings = bookings ?? throw new ArgumentNullException(nameof(bookings));
        _parking = parking ?? throw new ArgumentNullException(nameof(parking));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public IReadOnlyList<Booking> GetBookingsForDay(DateTime day)
    {
        BookingRules.ExpireNoShows(_bookings, ParkingClock.Now(_timeProvider));
        return _bookings.GetByArrivalPeriod(day.Date, day.Date.AddDays(1));
    }

    public Booking CreateBooking(string carNumber, SpotCategory category, DateTime arrivalTime)
    {
        var normalized = CarNumberFormat.Normalize(carNumber);
        if (!CarNumberFormat.IsValid(normalized))
        {
            throw new ParkingOperationException($"Номер автомобиля указан неверно. Пример: {CarNumberFormat.Example}.");
        }

        var now = ParkingClock.Now(_timeProvider);
        if (arrivalTime.Date != now.Date)
        {
            throw new ParkingOperationException("Бронировать можно только на сегодня.");
        }

        if (arrivalTime < now)
        {
            throw new ParkingOperationException("Время прибытия уже прошло.");
        }

        if (_parking.GetActiveSessions(normalized).Any(s => s.CarNumber == normalized))
        {
            throw new ParkingOperationException($"Автомобиль {normalized} уже находится на парковке.");
        }

        // GetOccupancy заодно снимает просроченные брони.
        var free = _parking.GetOccupancy().FirstOrDefault(o => o.Category == category)?.Free ?? 0;

        var existing = _bookings.FindWaitingByCarNumber(normalized);
        if (existing is not null)
        {
            var time = existing.ArrivalTime.ToString("HH:mm", CultureInfo.InvariantCulture);
            throw new ParkingOperationException($"У автомобиля {normalized} уже есть бронь на {time}.");
        }

        if (free <= 0)
        {
            throw new ParkingOperationException($"Свободных мест для брони нет ({category.ToDisplayName()}).");
        }

        var booking = new Booking
        {
            CarNumber = normalized,
            Category = category,
            ArrivalTime = new DateTime(arrivalTime.Year, arrivalTime.Month, arrivalTime.Day, arrivalTime.Hour, arrivalTime.Minute, 0, arrivalTime.Kind),
            CreatedTime = now,
            Status = BookingStatus.Waiting,
        };
        _bookings.Add(booking);
        return booking;
    }

    public void CancelBooking(int bookingId)
    {
        BookingRules.ExpireNoShows(_bookings, ParkingClock.Now(_timeProvider));
        var booking = _bookings.GetById(bookingId)
            ?? throw new ParkingOperationException("Бронь не найдена.");

        if (booking.Status != BookingStatus.Waiting)
        {
            throw new ParkingOperationException("Бронь уже не активна.");
        }

        booking.Status = BookingStatus.Cancelled;
        _bookings.Update(booking);
    }
}
