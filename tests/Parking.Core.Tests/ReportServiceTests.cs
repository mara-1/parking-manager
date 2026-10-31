using Parking.Core.Models;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "10")]
public class ReportServiceTests
{
    private readonly ManualTimeProvider _clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0));
    private readonly FakeBookingRepository _bookingRepository = new FakeBookingRepository();
    private readonly ParkingService _parking;
    private readonly BookingService _bookings;
    private readonly ReportService _reports;

    public ReportServiceTests()
    {
        var sessions = new FakeSessionRepository();
        var categories = new FakeCategoryRepository(regular: 10, disabled: 2);
        _parking = new ParkingService(categories, sessions, new FakeTariffRepository(regular: 60m, disabled: 0m), _bookingRepository, _clock);
        _bookings = new BookingService(_bookingRepository, _parking, _clock);
        _reports = new ReportService(sessions, _bookingRepository, _clock);
    }

    private static DateTime Day => new DateTime(2026, 10, 24);

    [Fact]
    public void Build_Today_CountsSessionsRevenueAndAverageStay()
    {
        Park("А123ВС73", SpotCategory.Regular, minutes: 45);
        Park("В456ОР73", SpotCategory.Regular, minutes: 95);
        _parking.RegisterEntry("Е789КМ73", SpotCategory.Regular);

        var report = _reports.Build(Day, Day);

        Assert.Equal((3, 2, 1), (report.TotalSessions, report.FinishedSessions, report.ActiveSessions));
        Assert.Equal(180m, report.Revenue);
        Assert.Equal(TimeSpan.FromMinutes(70), report.AverageStay);
        Assert.Equal((Day, Day), (report.From, report.To));
    }

    [Fact]
    public void Build_GroupsByCategory()
    {
        Park("А123ВС73", SpotCategory.Regular, minutes: 61);
        Park("М808АА73", SpotCategory.Disabled, minutes: 180);

        var report = _reports.Build(Day, Day);

        var regular = report.Categories.Single(c => c.Category == SpotCategory.Regular);
        var disabled = report.Categories.Single(c => c.Category == SpotCategory.Disabled);
        Assert.Equal((1, 1, 120m), (regular.Sessions, regular.Finished, regular.Revenue));
        Assert.Equal(TimeSpan.FromMinutes(61), regular.AverageStay);
        Assert.Equal((1, 1, 0m), (disabled.Sessions, disabled.Finished, disabled.Revenue));
        Assert.Equal(TimeSpan.FromHours(3), disabled.AverageStay);
    }

    [Fact]
    public void Build_SeveralDays_ListsEveryDayIncludingEmpty()
    {
        Park("А123ВС73", SpotCategory.Regular, minutes: 30);
        _clock.Advance(TimeSpan.FromDays(2));
        Park("В456ОР73", SpotCategory.Regular, minutes: 150);

        var report = _reports.Build(Day, Day.AddDays(2));

        Assert.Collection(
            report.Days,
            first => Assert.Equal((Day, 1, 60m), (first.Date, first.Entries, first.Revenue)),
            empty => Assert.Equal((Day.AddDays(1), 0, 0m), (empty.Date, empty.Entries, empty.Revenue)),
            last => Assert.Equal((Day.AddDays(2), 1, 180m), (last.Date, last.Entries, last.Revenue)));
        Assert.Equal(240m, report.Revenue);
    }

    [Fact]
    public void Build_OtherPeriod_DoesNotCountSessions()
    {
        Park("А123ВС73", SpotCategory.Regular, minutes: 30);

        var report = _reports.Build(Day.AddDays(1), Day.AddDays(3));

        Assert.Equal(0, report.TotalSessions);
        Assert.Equal(0m, report.Revenue);
        Assert.Equal(3, report.Days.Count);
    }

    [Fact]
    public void Build_OnlyActiveSessions_NoRevenueAndZeroAverage()
    {
        _parking.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _clock.Advance(TimeSpan.FromHours(2));

        var report = _reports.Build(Day, Day);

        Assert.Equal(1, report.ActiveSessions);
        Assert.Equal(0m, report.Revenue);
        Assert.Equal(TimeSpan.Zero, report.AverageStay);
    }

    [Fact]
    [Trait("Task", "9")]
    public void Build_CountsBookingsByStatus()
    {
        _bookings.CreateBooking("А123ВС73", SpotCategory.Regular, At(10, 0));
        _bookings.CreateBooking("В456ОР73", SpotCategory.Regular, At(10, 30));
        var cancelled = _bookings.CreateBooking("Е789КМ73", SpotCategory.Regular, At(11, 0));
        _bookings.CancelBooking(cancelled.Id);
        _clock.Advance(TimeSpan.FromHours(1));
        _parking.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _clock.Advance(TimeSpan.FromMinutes(50));
        _bookings.CreateBooking("К321МН73", SpotCategory.Disabled, At(12, 0));

        var report = _reports.Build(Day, Day);

        Assert.Equal(4, report.BookingsTotal);
        Assert.Equal(1, report.BookingsArrived);
        Assert.Equal(1, report.BookingsNoShow);
        Assert.Equal(1, report.BookingsCancelled);
        Assert.Equal(1, report.BookingsWaiting);
    }

    [Fact]
    public void Build_EndBeforeStart_Throws()
    {
        var error = Assert.Throws<ParkingOperationException>(() => _reports.Build(Day, Day.AddDays(-1)));

        Assert.Equal("Дата окончания периода раньше даты начала.", error.Message);
    }

    [Fact]
    public void Build_PeriodLongerThanYear_Throws()
    {
        Assert.Throws<ParkingOperationException>(() => _reports.Build(new DateTime(2026, 1, 1), new DateTime(2027, 1, 5)));
    }

    private static DateTime At(int hour, int minute)
    {
        return Day.AddHours(hour).AddMinutes(minute);
    }

    // Въезд сейчас и выезд через заданное число минут.
    private void Park(string carNumber, SpotCategory category, int minutes)
    {
        var session = _parking.RegisterEntry(carNumber, category);
        _clock.Advance(TimeSpan.FromMinutes(minutes));
        _parking.RegisterExit(session.Id);
    }
}
