using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Services;

// Отчётность и статистика: количество стоянок, выручка, средняя длительность,
// разбивка по категориям мест и по дням, итоги по броням.
public sealed class ReportService : IReportService
{
    public const int MaxPeriodDays = 366;

    private readonly ISessionRepository _sessions;
    private readonly IBookingRepository _bookings;
    private readonly TimeProvider _timeProvider;

    public ReportService(ISessionRepository sessions, IBookingRepository bookings, TimeProvider timeProvider)
    {
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _bookings = bookings ?? throw new ArgumentNullException(nameof(bookings));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public PeriodReport Build(DateTime fromDay, DateTime toDay)
    {
        var from = fromDay.Date;
        var to = toDay.Date;
        if (to < from)
        {
            throw new ParkingOperationException("Дата окончания периода раньше даты начала.");
        }

        var dayCount = (to - from).Days + 1;
        if (dayCount > MaxPeriodDays)
        {
            throw new ParkingOperationException($"Период отчёта не должен превышать {MaxPeriodDays} дней.");
        }

        var now = ParkingClock.Now(_timeProvider);
        BookingRules.ExpireNoShows(_bookings, now);

        var until = to.AddDays(1);
        var sessions = _sessions.GetByEntryPeriod(from, until, string.Empty);
        var finished = sessions.Where(s => !s.IsActive).ToList();
        var bookings = _bookings.GetByArrivalPeriod(from, until);

        return new PeriodReport
        {
            From = from,
            To = to,
            CreatedAt = now,
            TotalSessions = sessions.Count,
            FinishedSessions = finished.Count,
            ActiveSessions = sessions.Count - finished.Count,
            Revenue = finished.Sum(s => s.Amount ?? 0m),
            AverageStay = AverageStay(finished),
            BookingsTotal = bookings.Count,
            BookingsArrived = bookings.Count(b => b.Status == BookingStatus.Arrived),
            BookingsNoShow = bookings.Count(b => b.Status == BookingStatus.NoShow),
            BookingsCancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled),
            BookingsWaiting = bookings.Count(b => b.Status == BookingStatus.Waiting),
            Categories = Enum.GetValues<SpotCategory>().Select(c => CategoryRow(c, sessions)).ToList(),
            Days = Enumerable.Range(0, dayCount).Select(i => DayRow(from.AddDays(i), sessions)).ToList(),
        };
    }

    private static ReportCategoryRow CategoryRow(SpotCategory category, IReadOnlyList<ParkingSession> sessions)
    {
        var ofCategory = sessions.Where(s => s.Category == category).ToList();
        var finished = ofCategory.Where(s => !s.IsActive).ToList();
        return new ReportCategoryRow
        {
            Category = category,
            Sessions = ofCategory.Count,
            Finished = finished.Count,
            Revenue = finished.Sum(s => s.Amount ?? 0m),
            AverageStay = AverageStay(finished),
        };
    }

    private static ReportDayRow DayRow(DateTime day, IReadOnlyList<ParkingSession> sessions)
    {
        var ofDay = sessions.Where(s => s.EntryTime.Date == day).ToList();
        return new ReportDayRow
        {
            Date = day,
            Entries = ofDay.Count,
            Revenue = ofDay.Where(s => !s.IsActive).Sum(s => s.Amount ?? 0m),
        };
    }

    // Средняя длительность завершённых стоянок, округлённая до минуты.
    private static TimeSpan AverageStay(IReadOnlyCollection<ParkingSession> finished)
    {
        if (finished.Count == 0)
        {
            return TimeSpan.Zero;
        }

        var minutes = finished.Average(s => (s.ExitTime!.Value - s.EntryTime).TotalMinutes);
        return TimeSpan.FromMinutes(Math.Round(minutes));
    }
}
