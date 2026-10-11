using Parking.Core.Models;
using Parking.Core.Services;
using ParkingManager.Tests.Fakes;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
public class MainViewModelTests
{
    public const int SpotCount = 5;

    private readonly FakeAuthService _authService = new FakeAuthService();
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _authService.Login(FakeAuthService.ValidLogin, FakeAuthService.ValidPassword);
        var parkingService = new ParkingService(new InMemorySpotRepository(SpotCount));
        _viewModel = new MainViewModel(_authService, parkingService);
    }

    [Fact]
    [Trait("Task", "6")]
    public void Constructor_LoadsAllSpotsAsFree()
    {
        Assert.Equal(SpotCount, _viewModel.Spots.Count);
        Assert.Equal(SpotCount, _viewModel.FreeCount);
        Assert.Equal(0, _viewModel.OccupiedCount);
    }

    [Fact]
    [Trait("Task", "5")]
    public void OperatorName_ShowsCurrentOperatorFullName()
    {
        Assert.Equal(FakeAuthService.OperatorFullName, _viewModel.OperatorName);
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntryCommand_NoSpotSelected_CannotExecute()
    {
        _viewModel.CarNumber = "А123ВС73";

        Assert.False(_viewModel.RegisterEntryCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntryCommand_EmptyCarNumber_CannotExecute()
    {
        _viewModel.SelectedSpot = _viewModel.Spots[0];
        _viewModel.CarNumber = "  ";

        Assert.False(_viewModel.RegisterEntryCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntry_FreeSpot_OccupiesSpotAndUpdatesCounters()
    {
        _viewModel.SelectedSpot = _viewModel.Spots[0];
        _viewModel.CarNumber = "а123вс73";

        _viewModel.RegisterEntryCommand.Execute(null);

        var spot = _viewModel.Spots[0];
        Assert.True(spot.IsOccupied);
        Assert.Equal("А123ВС73", spot.CarNumber);
        Assert.Equal(SpotCount - 1, _viewModel.FreeCount);
        Assert.Equal(1, _viewModel.OccupiedCount);
        Assert.Equal(string.Empty, _viewModel.CarNumber);
        Assert.Equal("Въезд зарегистрирован: место A-01.", _viewModel.StatusMessage);
        Assert.Same(spot, _viewModel.SelectedSpot);
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntry_CarAlreadyParked_ShowsErrorMessage()
    {
        _viewModel.SelectedSpot = _viewModel.Spots[0];
        _viewModel.CarNumber = "А123ВС73";
        _viewModel.RegisterEntryCommand.Execute(null);

        _viewModel.SelectedSpot = _viewModel.Spots[1];
        _viewModel.CarNumber = "А123ВС73";
        _viewModel.RegisterEntryCommand.Execute(null);

        Assert.True(_viewModel.Spots[1].IsFree);
        Assert.Equal("Автомобиль А123ВС73 уже находится на парковке.", _viewModel.StatusMessage);
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterExitCommand_FreeSpot_CannotExecute()
    {
        _viewModel.SelectedSpot = _viewModel.Spots[0];

        Assert.False(_viewModel.RegisterExitCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterExit_OccupiedSpot_FreesSpot()
    {
        _viewModel.SelectedSpot = _viewModel.Spots[0];
        _viewModel.CarNumber = "А123ВС73";
        _viewModel.RegisterEntryCommand.Execute(null);

        _viewModel.RegisterExitCommand.Execute(null);

        Assert.Equal(SpotStatus.Free, _viewModel.Spots[0].Status);
        Assert.Equal(SpotCount, _viewModel.FreeCount);
        Assert.Equal("Выезд зарегистрирован: место A-01 освобождено.", _viewModel.StatusMessage);
    }
}
