using Parking.Core.Models;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
[Trait("Task", "8")]
public class SettingsViewModelTests
{
    private readonly FakeCategoryRepository _categories = new FakeCategoryRepository(regular: 90, disabled: 10);
    private readonly FakeTariffRepository _tariffs = new FakeTariffRepository(regular: 60m, disabled: 0m);
    private readonly SettingsViewModel _viewModel;

    public SettingsViewModelTests()
    {
        var clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0));
        var parking = new ParkingService(_categories, new FakeSessionRepository(), _tariffs, new FakeBookingRepository(), clock);
        _viewModel = new SettingsViewModel(new SettingsService(_categories, _tariffs, parking));
    }

    [Fact]
    public void Load_ShowsCurrentCapacitiesAndPrices()
    {
        Assert.Equal("90", _viewModel.RegularCapacityText);
        Assert.Equal("10", _viewModel.DisabledCapacityText);
        Assert.Equal("60", _viewModel.RegularPriceText);
        Assert.Equal("0", _viewModel.DisabledPriceText);
    }

    [Fact]
    public void Save_PriceWithComma_SavesDecimalPrice()
    {
        var saved = false;
        _viewModel.Saved += (sender, args) => saved = true;
        _viewModel.RegularPriceText = "75,5";

        _viewModel.SaveCommand.Execute(null);

        Assert.True(saved);
        Assert.Equal(75.5m, _tariffs.Find(SpotCategory.Regular)!.PricePerHour);
        Assert.Equal("Настройки сохранены.", _viewModel.Message);
    }

    [Fact]
    public void Save_NotANumber_ShowsErrorAndSavesNothing()
    {
        _viewModel.RegularCapacityText = "много";

        _viewModel.SaveCommand.Execute(null);

        Assert.Equal("Вместимость должна быть целым числом.", _viewModel.Message);
        Assert.Equal(90, _categories.Find(SpotCategory.Regular)!.Capacity);
    }

    [Fact]
    [Trait("Task", "6")]
    public void Save_TooFewDisabledPlaces_ShowsRuleMessage()
    {
        _viewModel.DisabledCapacityText = "1";

        _viewModel.SaveCommand.Execute(null);

        Assert.Equal("Мест для инвалидов должно быть не менее 10 % от общего числа: не менее 10.", _viewModel.Message);
    }
}
