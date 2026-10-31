using Parking.Core.Models;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "8")]
public class SettingsServiceTests
{
    private readonly FakeCategoryRepository _categories = new FakeCategoryRepository(regular: 90, disabled: 10);
    private readonly FakeTariffRepository _tariffs = new FakeTariffRepository(regular: 60m, disabled: 0m);
    private readonly ParkingService _parking;
    private readonly SettingsService _settings;

    public SettingsServiceTests()
    {
        var clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0));
        _parking = new ParkingService(_categories, new FakeSessionRepository(), _tariffs, new FakeBookingRepository(), clock);
        _settings = new SettingsService(_categories, _tariffs, _parking);
    }

    [Fact]
    public void GetSettings_ReturnsCapacityPriceAndPlacesInUse()
    {
        _parking.RegisterEntry("А123ВС73", SpotCategory.Regular);

        var regular = _settings.GetSettings().Single(s => s.Category == SpotCategory.Regular);

        Assert.Equal((90, 60m, 1), (regular.Capacity, regular.PricePerHour, regular.InUse));
    }

    [Fact]
    [Trait("Task", "6")]
    public void UpdateCapacities_Valid_ChangesFreePlaces()
    {
        _settings.UpdateCapacities(100, 12);

        var occupancy = _parking.GetOccupancy();
        Assert.Equal(100, occupancy.Single(o => o.Category == SpotCategory.Regular).Free);
        Assert.Equal(12, occupancy.Single(o => o.Category == SpotCategory.Disabled).Free);
    }

    [Fact]
    [Trait("Task", "6")]
    public void UpdateCapacities_BelowOccupied_Throws()
    {
        _parking.RegisterEntry("А111АА73", SpotCategory.Disabled);
        _parking.RegisterEntry("В222ВВ73", SpotCategory.Disabled);

        Assert.Throws<ParkingOperationException>(() => _settings.UpdateCapacities(0, 1));
        Assert.Equal(10, _categories.Find(SpotCategory.Disabled)!.Capacity);
    }

    [Fact]
    [Trait("Task", "6")]
    public void UpdateCapacities_DisabledBelowTenPercent_Throws()
    {
        var error = Assert.Throws<ParkingOperationException>(() => _settings.UpdateCapacities(450, 10));

        Assert.Equal("Мест для инвалидов должно быть не менее 10 % от общего числа: не менее 46.", error.Message);
        Assert.Equal(90, _categories.Find(SpotCategory.Regular)!.Capacity);
    }

    [Theory]
    [InlineData(500, 50)]
    [InlineData(100, 10)]
    [InlineData(11, 2)]
    [InlineData(5, 1)]
    public void MinimumDisabledPlaces_TenPercentRoundedUp(int total, int expected)
    {
        Assert.Equal(expected, SettingsService.MinimumDisabledPlaces(total));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10001)]
    public void UpdatePrice_OutOfRange_Throws(int price)
    {
        Assert.Throws<ParkingOperationException>(() => _settings.UpdatePrice(SpotCategory.Regular, price));
    }

    [Fact]
    public void UpdatePrice_Valid_AppliesToNextExit()
    {
        var session = _parking.RegisterEntry("А123ВС73", SpotCategory.Regular);

        _settings.UpdatePrice(SpotCategory.Regular, 80m);

        Assert.Equal(80m, _parking.RegisterExit(session.Id).Amount);
    }
}
