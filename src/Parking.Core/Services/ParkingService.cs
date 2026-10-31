using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Services;

public class ParkingService : IParkingService
{
    private readonly ICategoryRepository _categories;
    private readonly ISessionRepository _sessions;
    private readonly ITariffRepository _tariffs;
    private readonly IBookingRepository _bookings;
    private readonly TimeProvider _timeProvider;

    public ParkingService(
        ICategoryRepository categories,
        ISessionRepository sessions,
        ITariffRepository tariffs,
        IBookingRepository bookings,
        TimeProvider timeProvider)
    {
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _tariffs = tariffs ?? throw new ArgumentNullException(nameof(tariffs));
        _bookings = bookings ?? throw new ArgumentNullException(nameof(bookings));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public IReadOnlyList<OccupancyInfo> GetOccupancy()
    {
        BookingRules.ExpireNoShows(_bookings, Now());
        return _categories.GetAll()
            .Select(c => new OccupancyInfo
            {
                Category = c.Category,
                Capacity = c.Capacity,
                Occupied = _sessions.CountActive(c.Category),
                Booked = _bookings.CountWaiting(c.Category),
            })
            .ToList();
    }

    public IReadOnlyList<ParkingSession> GetActiveSessions(string carNumberPart)
    {
        return _sessions.GetActive(CarNumberFormat.Normalize(carNumberPart));
    }

    public ParkingSession RegisterEntry(string carNumber, SpotCategory category)
    {
        var normalized = CarNumberFormat.RequireValid(carNumber);
        EnsureNotParked(normalized);

        var now = Now();
        BookingRules.ExpireNoShows(_bookings, now);

        // Автомобиль с бронью занимает удержанное для него место своей категории.
        var booking = _bookings.FindWaitingByCarNumber(normalized);
        if (booking is not null)
        {
            category = booking.Category;
        }

        EnsurePlaceAvailable(category, booking);

        var session = new ParkingSession
        {
            CarNumber = normalized,
            Category = category,
            EntryTime = now,
        };
        _sessions.Add(session);

        if (booking is not null)
        {
            booking.Status = BookingStatus.Arrived;
            booking.SessionId = session.Id;
            _bookings.Update(booking);
        }

        return session;
    }

    public ParkingSession RegisterExit(int sessionId)
    {
        var session = _sessions.GetById(sessionId)
            ?? throw new ParkingOperationException("Стоянка не найдена.");

        if (!session.IsActive)
        {
            throw new ParkingOperationException($"Выезд автомобиля {session.CarNumber} уже зарегистрирован.");
        }

        session.ExitTime = Now();
        session.Amount = Calculate(session, session.ExitTime.Value);
        _sessions.Update(session);
        return session;
    }

    public decimal CalculateAmount(ParkingSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.Amount ?? Calculate(session, Now());
    }

    public IReadOnlyList<ParkingSession> GetHistory(DateTime from, DateTime until, string carNumberPart)
    {
        if (until <= from)
        {
            throw new ParkingOperationException("Конец периода должен быть позже начала.");
        }

        return _sessions.GetByEntryPeriod(from, until, CarNumberFormat.Normalize(carNumberPart));
    }

    private void EnsureNotParked(string carNumber)
    {
        if (_sessions.FindActiveByCarNumber(carNumber) is not null)
        {
            throw new ParkingOperationException($"Автомобиль {carNumber} уже находится на парковке.");
        }
    }

    // Свободное место есть, если занятые плюс удержанные бронями других автомобилей
    // меньше вместимости. Своя бронь автомобиля место не отнимает: оно удержано для него.
    private void EnsurePlaceAvailable(SpotCategory category, Booking? ownBooking)
    {
        var capacity = _categories.Find(category)
            ?? throw new ParkingOperationException("Категория мест не настроена.");
        var heldForOthers = _bookings.CountWaiting(category) - (ownBooking is null ? 0 : 1);
        if (_sessions.CountActive(category) + heldForOthers >= capacity.Capacity)
        {
            throw new ParkingOperationException($"Свободных мест нет ({category.ToDisplayName()}).");
        }
    }

    // Тариф берётся действующий: изменение тарифа применяется к ещё не оплаченным стоянкам.
    private decimal Calculate(ParkingSession session, DateTime until)
    {
        var price = _tariffs.Find(session.Category)?.PricePerHour ?? 0m;
        return TariffCalculator.Calculate(until - session.EntryTime, price);
    }

    private DateTime Now()
    {
        return ParkingClock.Now(_timeProvider);
    }
}
