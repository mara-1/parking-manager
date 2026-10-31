using Parking.Core.DataAccess;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;

namespace Parking.Core.Tests;

// Службы и хранилища не создаются без своих зависимостей: ошибка конфигурации
// обнаруживается сразу при запуске, а не при первой операции оператора.
[Trait("Category", "Unit")]
[Trait("Task", "2")]
public class ConstructorGuardTests
{
    private readonly FakeCategoryRepository _categories = new FakeCategoryRepository(regular: 10, disabled: 2);
    private readonly FakeSessionRepository _sessions = new FakeSessionRepository();
    private readonly FakeTariffRepository _tariffs = new FakeTariffRepository();
    private readonly FakeBookingRepository _bookings = new FakeBookingRepository();
    private readonly ManualTimeProvider _clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0));

    [Fact]
    public void ParkingService_NullDependency_Throws()
    {
        Assert.Throws<ArgumentNullException>("categories", () => new ParkingService(null!, _sessions, _tariffs, _bookings, _clock));
        Assert.Throws<ArgumentNullException>("sessions", () => new ParkingService(_categories, null!, _tariffs, _bookings, _clock));
        Assert.Throws<ArgumentNullException>("tariffs", () => new ParkingService(_categories, _sessions, null!, _bookings, _clock));
        Assert.Throws<ArgumentNullException>("bookings", () => new ParkingService(_categories, _sessions, _tariffs, null!, _clock));
        Assert.Throws<ArgumentNullException>("timeProvider", () => new ParkingService(_categories, _sessions, _tariffs, _bookings, null!));
    }

    [Fact]
    public void BookingAndSettingsServices_NullDependency_Throw()
    {
        var parking = new ParkingService(_categories, _sessions, _tariffs, _bookings, _clock);

        Assert.Throws<ArgumentNullException>("bookings", () => new BookingService(null!, parking, _clock));
        Assert.Throws<ArgumentNullException>("parking", () => new BookingService(_bookings, null!, _clock));
        Assert.Throws<ArgumentNullException>("timeProvider", () => new BookingService(_bookings, parking, null!));
        Assert.Throws<ArgumentNullException>("categories", () => new SettingsService(null!, _tariffs, parking));
        Assert.Throws<ArgumentNullException>("tariffs", () => new SettingsService(_categories, null!, parking));
        Assert.Throws<ArgumentNullException>("parking", () => new SettingsService(_categories, _tariffs, null!));
    }

    [Fact]
    [Trait("Task", "10")]
    public void ReportService_NullDependency_Throws()
    {
        Assert.Throws<ArgumentNullException>("sessions", () => new ReportService(null!, _bookings, _clock));
        Assert.Throws<ArgumentNullException>("bookings", () => new ReportService(_sessions, null!, _clock));
        Assert.Throws<ArgumentNullException>("timeProvider", () => new ReportService(_sessions, _bookings, null!));
    }

    [Fact]
    [Trait("Task", "5")]
    public void AuthService_NullRepository_Throws()
    {
        Assert.Throws<ArgumentNullException>("operators", () => new AuthService(null!));
    }

    [Fact]
    [Trait("Task", "3")]
    public void Repositories_NullContext_Throw()
    {
        Assert.Throws<ArgumentNullException>("dbContext", () => new OperatorRepository(null!));
        Assert.Throws<ArgumentNullException>("dbContext", () => new CategoryRepository(null!));
        Assert.Throws<ArgumentNullException>("dbContext", () => new TariffRepository(null!));
        Assert.Throws<ArgumentNullException>("dbContext", () => new SessionRepository(null!));
        Assert.Throws<ArgumentNullException>("dbContext", () => new BookingRepository(null!));
    }
}
