using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Services;

public class ParkingService : IParkingService
{
    private readonly ICategoryRepository _categories;
    private readonly ISessionRepository _sessions;
    private readonly ITariffRepository _tariffs;
    private readonly TimeProvider _timeProvider;

    public ParkingService(ICategoryRepository categories, ISessionRepository sessions, ITariffRepository tariffs, TimeProvider timeProvider)
    {
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _tariffs = tariffs ?? throw new ArgumentNullException(nameof(tariffs));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public IReadOnlyList<OccupancyInfo> GetOccupancy()
    {
        return _categories.GetAll()
            .Select(c => new OccupancyInfo
            {
                Category = c.Category,
                Capacity = c.Capacity,
                Occupied = _sessions.CountActive(c.Category),
            })
            .ToList();
    }

    public IReadOnlyList<ParkingSession> GetActiveSessions(string carNumberPart)
    {
        return _sessions.GetActive(CarNumberFormat.Normalize(carNumberPart));
    }

    public ParkingSession RegisterEntry(string carNumber, SpotCategory category)
    {
        var normalized = CarNumberFormat.Normalize(carNumber);
        if (normalized.Length == 0)
        {
            throw new ParkingOperationException("Не указан номер автомобиля.");
        }

        if (!CarNumberFormat.IsValid(normalized))
        {
            throw new ParkingOperationException($"Номер автомобиля указан неверно. Пример: {CarNumberFormat.Example}.");
        }

        if (_sessions.FindActiveByCarNumber(normalized) is not null)
        {
            throw new ParkingOperationException($"Автомобиль {normalized} уже находится на парковке.");
        }

        var capacity = _categories.Find(category)
            ?? throw new ParkingOperationException("Категория мест не настроена.");
        if (_sessions.CountActive(category) >= capacity.Capacity)
        {
            throw new ParkingOperationException($"Свободных мест нет ({category.ToDisplayName()}).");
        }

        var session = new ParkingSession
        {
            CarNumber = normalized,
            Category = category,
            EntryTime = Now(),
        };
        _sessions.Add(session);
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

    // Тариф берётся действующий: изменение тарифа применяется к ещё не оплаченным стоянкам.
    private decimal Calculate(ParkingSession session, DateTime until)
    {
        var price = _tariffs.Find(session.Category)?.PricePerHour ?? 0m;
        return TariffCalculator.Calculate(until - session.EntryTime, price);
    }

    // Секунды отбрасываются: оператору и в отчётах нужна точность до минуты.
    private DateTime Now()
    {
        var now = _timeProvider.GetLocalNow().DateTime;
        return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
    }
}
