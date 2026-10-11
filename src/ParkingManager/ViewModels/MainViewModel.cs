using System.Collections.ObjectModel;
using System.Windows.Input;
using Parking.Core.Models;
using Parking.Core.Services;

namespace ParkingManager.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IParkingService _parkingService;
    private SpotViewModel? _selectedSpot;
    private string _carNumber = string.Empty;
    private string _statusMessage = string.Empty;

    public MainViewModel(IAuthService authService, IParkingService parkingService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _parkingService = parkingService ?? throw new ArgumentNullException(nameof(parkingService));

        Spots = new ObservableCollection<SpotViewModel>();
        RegisterEntryCommand = new RelayCommand(RegisterEntry, CanRegisterEntry);
        RegisterExitCommand = new RelayCommand(RegisterExit, CanRegisterExit);
        LoadSpots();
    }

    public string OperatorName => _authService.CurrentOperator?.FullName ?? string.Empty;

    public ObservableCollection<SpotViewModel> Spots { get; }

    public SpotViewModel? SelectedSpot
    {
        get => _selectedSpot;
        set
        {
            if (SetProperty(ref _selectedSpot, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string CarNumber
    {
        get => _carNumber;
        set
        {
            if (SetProperty(ref _carNumber, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public int FreeCount => Spots.Count(s => s.Status == SpotStatus.Free);

    public int OccupiedCount => Spots.Count(s => s.Status == SpotStatus.Occupied);

    public ICommand RegisterEntryCommand { get; }

    public ICommand RegisterExitCommand { get; }

    private void LoadSpots()
    {
        var selectedId = SelectedSpot?.Id;

        Spots.Clear();
        foreach (var spot in _parkingService.GetSpots())
        {
            Spots.Add(new SpotViewModel(spot));
        }

        SelectedSpot = Spots.FirstOrDefault(s => s.Id == selectedId);
        OnPropertyChanged(nameof(FreeCount));
        OnPropertyChanged(nameof(OccupiedCount));
        CommandManager.InvalidateRequerySuggested();
    }

    private bool CanRegisterEntry()
    {
        return SelectedSpot is { IsFree: true } && !string.IsNullOrWhiteSpace(CarNumber);
    }

    private bool CanRegisterExit()
    {
        return SelectedSpot is { IsOccupied: true };
    }

    private void RegisterEntry()
    {
        var spot = SelectedSpot;
        if (spot is null)
        {
            return;
        }

        try
        {
            _parkingService.RegisterEntry(spot.Id, CarNumber);
            StatusMessage = $"Въезд зарегистрирован: место {spot.Number}.";
            CarNumber = string.Empty;
            LoadSpots();
        }
        catch (ArgumentException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private void RegisterExit()
    {
        var spot = SelectedSpot;
        if (spot is null)
        {
            return;
        }

        try
        {
            _parkingService.RegisterExit(spot.Id);
            StatusMessage = $"Выезд зарегистрирован: место {spot.Number} освобождено.";
            LoadSpots();
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
    }
}
