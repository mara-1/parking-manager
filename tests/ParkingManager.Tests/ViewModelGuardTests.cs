using Parking.Core.Services;
using Parking.Core.Tests.Fakes;
using ParkingManager.Tests.Fakes;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

// Модели представления не создаются без служб, от которых зависят.
[Trait("Category", "Unit")]
[Trait("Task", "4")]
public class ViewModelGuardTests
{
    private readonly ManualTimeProvider _clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0));
    private readonly FakeAuthService _auth = new FakeAuthService();
    private readonly ParkingService _parking;
    private readonly SettingsService _settings;
    private readonly BookingService _bookings;
    private readonly ReportService _reports;

    public ViewModelGuardTests()
    {
        var categories = new FakeCategoryRepository(regular: 10, disabled: 2);
        var sessions = new FakeSessionRepository();
        var tariffs = new FakeTariffRepository();
        var bookings = new FakeBookingRepository();
        _parking = new ParkingService(categories, sessions, tariffs, bookings, _clock);
        _settings = new SettingsService(categories, tariffs, _parking);
        _bookings = new BookingService(bookings, _parking, _clock);
        _reports = new ReportService(sessions, bookings, _clock);
    }

    [Fact]
    public void MainViewModel_NullDependency_Throws()
    {
        var reports = Reports();

        Assert.Throws<ArgumentNullException>("authService", () => new MainViewModel(null!, _parking, _settings, _bookings, reports, _clock));
        Assert.Throws<ArgumentNullException>("parkingService", () => new MainViewModel(_auth, null!, _settings, _bookings, reports, _clock));
        Assert.Throws<ArgumentNullException>("settingsService", () => new MainViewModel(_auth, _parking, null!, _bookings, reports, _clock));
        Assert.Throws<ArgumentNullException>("bookingService", () => new MainViewModel(_auth, _parking, _settings, null!, reports, _clock));
        Assert.Throws<ArgumentNullException>("reports", () => new MainViewModel(_auth, _parking, _settings, _bookings, null!, _clock));
        Assert.Throws<ArgumentNullException>("timeProvider", () => new MainViewModel(_auth, _parking, _settings, _bookings, reports, null!));
    }

    [Fact]
    public void TabViewModels_NullDependency_Throw()
    {
        Assert.Throws<ArgumentNullException>("parkingService", () => new HistoryViewModel(null!, _clock));
        Assert.Throws<ArgumentNullException>("timeProvider", () => new HistoryViewModel(_parking, null!));
        Assert.Throws<ArgumentNullException>("bookingService", () => new BookingsViewModel(null!, _clock));
        Assert.Throws<ArgumentNullException>("timeProvider", () => new BookingsViewModel(_bookings, null!));
        Assert.Throws<ArgumentNullException>("settingsService", () => new SettingsViewModel(null!));
        Assert.Throws<ArgumentNullException>("authService", () => new LoginViewModel(null!));
    }

    [Fact]
    [Trait("Task", "10")]
    public void ReportsViewModel_NullDependency_Throws()
    {
        var exporter = new FakeReportExporter();
        var dialog = new FakeSaveFileDialog(null);

        Assert.Throws<ArgumentNullException>("reportService", () => new ReportsViewModel(null!, exporter, dialog, _clock));
        Assert.Throws<ArgumentNullException>("exporter", () => new ReportsViewModel(_reports, null!, dialog, _clock));
        Assert.Throws<ArgumentNullException>("saveFileDialog", () => new ReportsViewModel(_reports, exporter, null!, _clock));
        Assert.Throws<ArgumentNullException>("timeProvider", () => new ReportsViewModel(_reports, exporter, dialog, null!));
    }

    [Fact]
    public void RelayCommand_NullAction_Throws()
    {
        Assert.Throws<ArgumentNullException>("execute", () => new RelayCommand(null!));
    }

    private ReportsViewModel Reports()
    {
        return new ReportsViewModel(_reports, new FakeReportExporter(), new FakeSaveFileDialog(null), _clock);
    }
}
