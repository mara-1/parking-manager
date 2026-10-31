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
        var normalized = CarNumberFormat.RequireValid(carNumber);
        var now = ParkingClock.Now(_timeProvider);
        EnsureArrivalToday(arrivalTime, now);
        EnsureNotParked(normalized);

        // GetOccupancy заодно снимает просроченные брони, поэтому вызывается до проверки броней.
        var free = FreePlaces(category);
        EnsureNoWaitingBooking(normalized);
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

    private static void EnsureArrivalToday(DateTime arrivalTime, DateTime now)
    {
        if (arrivalTime.Date != now.Date)
        {
            throw new ParkingOperationException("Бронировать можно только на сегодня.");
        }

        if (arrivalTime < now)
        {
            throw new ParkingOperationException("Время прибытия уже прошло.");
        }
    }

    private void EnsureNotParked(string carNumber)
    {
        if (_parking.GetActiveSessions(carNumber).Any(s => s.CarNumber == carNumber))
        {
            throw new ParkingOperationException($"Автомобиль {carNumber} уже находится на парковке.");
        }
    }

    private int FreePlaces(SpotCategory category)
    {
        return _parking.GetOccupancy().FirstOrDefault(o => o.Category == category)?.Free ?? 0;
    }

    private void EnsureNoWaitingBooking(string carNumber)
    {
        var existing = _bookings.FindWaitingByCarNumber(carNumber);
        if (existing is not null)
        {
            var time = existing.ArrivalTime.ToString("HH:mm", CultureInfo.InvariantCulture);
            throw new ParkingOperationException($"У автомобиля {carNumber} уже есть бронь на {time}.");
        }
    }
}
