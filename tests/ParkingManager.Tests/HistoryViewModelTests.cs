using Parking.Core.Models;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
[Trait("Task", "7")]
public class HistoryViewModelTests
{
    private readonly ManualTimeProvider _clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0));
    private readonly ParkingService _service;

    public HistoryViewModelTests()
    {
        _service = new ParkingService(new FakeCategoryRepository(regular: 10, disabled: 2), new FakeSessionRepository(), new FakeTariffRepository(), new FakeBookingRepository(), _clock);
    }

    [Fact]
    public void Show_Today_ListsFinishedAndActiveSessions()
    {
        var first = _service.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _clock.Advance(TimeSpan.FromMinutes(45));
        _service.RegisterExit(first.Id);
        _service.RegisterEntry("Х777ХХ73", SpotCategory.Disabled);
        var history = new HistoryViewModel(_service, _clock);

        history.Show();

        Assert.Equal(2, history.Sessions.Count);
        Assert.Equal("Найдено стоянок: 2 · сумма 60 ₽", history.Summary);
        var finished = history.Sessions.Single(s => s.CarNumber == "А123ВС73");
        Assert.Equal("24.10.2026 09:45", finished.ExitTimeText);
        Assert.Equal("45 мин", finished.DurationText);
        Assert.Equal("60 ₽", finished.AmountText);
        Assert.Equal("на парковке", history.Sessions.Single(s => s.CarNumber == "Х777ХХ73").ExitTimeText);
    }

    [Fact]
    public void Show_SearchText_FiltersByNumber()
    {
        _service.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _service.RegisterEntry("Х777ХХ73", SpotCategory.Regular);
        var history = new HistoryViewModel(_service, _clock) { SearchText = "а123" };

        history.Show();

        Assert.Equal("А123ВС73", Assert.Single(history.Sessions).CarNumber);
    }

    [Fact]
    public void Show_OtherDay_FindsNothing()
    {
        _service.RegisterEntry("А123ВС73", SpotCategory.Regular);
        var history = new HistoryViewModel(_service, _clock)
        {
            From = new DateTime(2026, 10, 20),
            To = new DateTime(2026, 10, 23),
        };

        history.Show();

        Assert.Empty(history.Sessions);
        Assert.Equal("Найдено стоянок: 0 · сумма 0 ₽", history.Summary);
    }

    [Fact]
    public void Show_EndBeforeStart_ShowsMessage()
    {
        var history = new HistoryViewModel(_service, _clock)
        {
            From = new DateTime(2026, 10, 25),
            To = new DateTime(2026, 10, 20),
        };

        history.Show();

        Assert.Equal("Дата окончания периода раньше даты начала.", history.Summary);
        Assert.Empty(history.Sessions);
    }

    [Fact]
    public void Show_NoDates_UsesToday()
    {
        _service.RegisterEntry("А123ВС73", SpotCategory.Regular);
        var history = new HistoryViewModel(_service, _clock) { From = null, To = null, SearchText = null! };

        history.ShowCommand.Execute(null);

        Assert.Single(history.Sessions);
        Assert.Equal(string.Empty, history.SearchText);
    }

    [Fact]
    public void SearchText_Changed_RaisesPropertyChanged()
    {
        var history = new HistoryViewModel(_service, _clock);
        var changed = new List<string?>();
        history.PropertyChanged += (sender, args) => changed.Add(args.PropertyName);

        history.SearchText = "777";

        Assert.Equal(new List<string?> { nameof(HistoryViewModel.SearchText) }, changed);
    }
}
