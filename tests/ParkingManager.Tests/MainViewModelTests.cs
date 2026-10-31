using Parking.Core.Models;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;
using ParkingManager.Tests.Fakes;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
public class MainViewModelTests
{
    private readonly FakeAuthService _authService = new FakeAuthService();
    private readonly ManualTimeProvider _clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0));
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _authService.Login(FakeAuthService.ValidLogin, FakeAuthService.ValidPassword);
        var categories = new FakeCategoryRepository(regular: 3, disabled: 1);
        var tariffs = new FakeTariffRepository(regular: 60m, disabled: 0m);
        var service = new ParkingService(categories, new FakeSessionRepository(), tariffs, _clock);
        _viewModel = new MainViewModel(_authService, service, new SettingsService(categories, tariffs, service), _clock);
    }

    [Fact]
    [Trait("Task", "6")]
    public void Constructor_ShowsCapacityPerCategory()
    {
        Assert.Equal(2, _viewModel.Occupancy.Count);
        Assert.Equal(3, _viewModel.RegularFree);
        Assert.Equal(1, _viewModel.DisabledFree);
        Assert.Equal(0, _viewModel.ActiveCount);
        Assert.Empty(_viewModel.Sessions);
    }

    [Fact]
    [Trait("Task", "5")]
    public void OperatorName_ShowsCurrentOperatorFullName()
    {
        Assert.Equal(FakeAuthService.OperatorFullName, _viewModel.OperatorName);
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntryCommand_EmptyCarNumber_CannotExecute()
    {
        _viewModel.CarNumber = "  ";

        Assert.False(_viewModel.RegisterEntryCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntry_ValidNumber_AddsSessionAndUpdatesCounters()
    {
        _viewModel.CarNumber = "а123вс73";

        _viewModel.RegisterEntryCommand.Execute(null);

        var session = Assert.Single(_viewModel.Sessions);
        Assert.Equal("А123ВС73", session.CarNumber);
        Assert.Equal("обычное", session.CategoryName);
        Assert.Equal("60 ₽", session.AmountText);
        Assert.Equal(2, _viewModel.RegularFree);
        Assert.Equal(1, _viewModel.ActiveCount);
        Assert.Equal(string.Empty, _viewModel.CarNumber);
        Assert.Same(session, _viewModel.SelectedSession);
        Assert.Equal("Въезд зарегистрирован: А123ВС73, место обычное.", _viewModel.StatusMessage);
    }

    [Fact]
    [Trait("Task", "6")]
    public void RegisterEntry_DisabledCategory_TakesDisabledPlace()
    {
        _viewModel.IsDisabledCategory = true;
        _viewModel.CarNumber = "М808АА73";

        _viewModel.RegisterEntryCommand.Execute(null);

        Assert.Equal(SpotCategory.Disabled, _viewModel.EntryCategory);
        Assert.False(_viewModel.IsRegularCategory);
        Assert.Equal(0, _viewModel.DisabledFree);
        Assert.Equal(3, _viewModel.RegularFree);
    }

    [Fact]
    [Trait("Task", "6")]
    public void RegisterEntryCommand_CategoryFull_CannotExecute()
    {
        _viewModel.IsDisabledCategory = true;
        _viewModel.CarNumber = "М808АА73";
        _viewModel.RegisterEntryCommand.Execute(null);

        _viewModel.CarNumber = "Н431ХА73";

        Assert.False(_viewModel.RegisterEntryCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntry_WrongNumber_ShowsErrorAndKeepsInput()
    {
        _viewModel.CarNumber = "12345";

        _viewModel.RegisterEntryCommand.Execute(null);

        Assert.Equal("Номер автомобиля указан неверно. Пример: А123ВС73.", _viewModel.StatusMessage);
        Assert.Equal("12345", _viewModel.CarNumber);
        Assert.Empty(_viewModel.Sessions);
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterExitCommand_NothingSelected_CannotExecute()
    {
        Assert.False(_viewModel.RegisterExitCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterExit_SelectedSession_RemovesCarAndFreesPlace()
    {
        _viewModel.CarNumber = "А123ВС73";
        _viewModel.RegisterEntryCommand.Execute(null);
        _clock.Advance(TimeSpan.FromHours(2));

        _viewModel.RegisterExitCommand.Execute(null);

        Assert.Empty(_viewModel.Sessions);
        Assert.Equal(3, _viewModel.RegularFree);
        Assert.Null(_viewModel.SelectedSession);
        Assert.Equal("Выезд зарегистрирован: А123ВС73, к оплате 120 ₽.", _viewModel.StatusMessage);
    }

    [Fact]
    [Trait("Task", "7")]
    public void SearchText_SingleMatch_FiltersListAndSelectsCar()
    {
        _viewModel.CarNumber = "А123ВС73";
        _viewModel.RegisterEntryCommand.Execute(null);
        _viewModel.CarNumber = "Х777ХХ73";
        _viewModel.RegisterEntryCommand.Execute(null);
        _viewModel.SelectedSession = null;

        _viewModel.SearchText = "777";

        var found = Assert.Single(_viewModel.Sessions);
        Assert.Equal("Х777ХХ73", found.CarNumber);
        Assert.Same(found, _viewModel.SelectedSession);
        Assert.True(_viewModel.RegisterExitCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Task", "6")]
    public void Occupancy_Cells_ShowOccupiedPlacesFirst()
    {
        _viewModel.CarNumber = "А123ВС73";
        _viewModel.RegisterEntryCommand.Execute(null);

        var regular = _viewModel.Occupancy.Single(o => o.Category == SpotCategory.Regular);

        Assert.Equal(3, regular.Cells.Count);
        Assert.Equal(CellState.Occupied, regular.Cells[0]);
        Assert.All(regular.Cells.Skip(1), cell => Assert.Equal(CellState.Free, cell));
        Assert.Equal("занято 1 · бронь 0 · свободно 2 из 3", regular.Summary);
    }

    [Fact]
    [Trait("Task", "8")]
    public void SettingsSaved_NewCapacity_RefreshesCounters()
    {
        _viewModel.Settings.RegularCapacityText = "9";

        _viewModel.Settings.SaveCommand.Execute(null);

        Assert.Equal("Настройки сохранены.", _viewModel.Settings.Message);
        Assert.Equal(9, _viewModel.RegularFree);
    }
}
