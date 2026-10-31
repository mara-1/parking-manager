using System.Collections.ObjectModel;
using System.Windows.Input;
using Parking.Core.Models;
using Parking.Core.Services;

namespace ParkingManager.ViewModels;

// Главное окно: занятость по категориям, регистрация въезда и выезда,
// список автомобилей на парковке с поиском, схема занятости и история.
public sealed class MainViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IParkingService _parkingService;
    private readonly TimeProvider _timeProvider;
    private SpotCategory _entryCategory = SpotCategory.Regular;
    private SessionViewModel? _selectedSession;
    private string _carNumber = string.Empty;
    private string _searchText = string.Empty;
    private string _statusMessage = string.Empty;

    public MainViewModel(
        IAuthService authService,
        IParkingService parkingService,
        ISettingsService settingsService,
        IBookingService bookingService,
        TimeProvider timeProvider)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _parkingService = parkingService ?? throw new ArgumentNullException(nameof(parkingService));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        Occupancy = new ObservableCollection<OccupancyViewModel>();
        Sessions = new ObservableCollection<SessionViewModel>();
        History = new HistoryViewModel(parkingService, timeProvider);
        Settings = new SettingsViewModel(settingsService);
        Settings.Saved += (sender, args) => Refresh(null);
        Bookings = new BookingsViewModel(bookingService, timeProvider);
        Bookings.Changed += (sender, args) => Refresh(null);
        RegisterEntryCommand = new RelayCommand(RegisterEntry, CanRegisterEntry);
        RegisterExitCommand = new RelayCommand(RegisterExit, CanRegisterExit);
        Refresh(null);
    }

    public string OperatorName => _authService.CurrentOperator?.FullName ?? string.Empty;

    public ObservableCollection<OccupancyViewModel> Occupancy { get; }

    public ObservableCollection<SessionViewModel> Sessions { get; }

    public HistoryViewModel History { get; }

    public SettingsViewModel Settings { get; }

    public BookingsViewModel Bookings { get; }

    public int BookedCount => Occupancy.Sum(o => o.Booked);

    public int RegularFree => FreeOf(SpotCategory.Regular);

    public int DisabledFree => FreeOf(SpotCategory.Disabled);

    public int ActiveCount => Occupancy.Sum(o => o.Occupied);

    public SpotCategory EntryCategory => _entryCategory;

    // Две радиокнопки категории въезда привязаны к этим свойствам.
    public bool IsRegularCategory
    {
        get => _entryCategory == SpotCategory.Regular;
        set
        {
            if (value)
            {
                SetEntryCategory(SpotCategory.Regular);
            }
        }
    }

    public bool IsDisabledCategory
    {
        get => _entryCategory == SpotCategory.Disabled;
        set
        {
            if (value)
            {
                SetEntryCategory(SpotCategory.Disabled);
            }
        }
    }

    public string CarNumber
    {
        get => _carNumber;
        set
        {
            if (SetProperty(ref _carNumber, value ?? string.Empty))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    // Поиск по части номера; если найден ровно один автомобиль, он выделяется,
    // и оператор сразу может зарегистрировать его выезд.
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value ?? string.Empty))
            {
                LoadSessions(null);
            }
        }
    }

    public SessionViewModel? SelectedSession
    {
        get => _selectedSession;
        set
        {
            if (SetProperty(ref _selectedSession, value))
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

    public ICommand RegisterEntryCommand { get; }

    public ICommand RegisterExitCommand { get; }

    // Перечитывает занятость и список; вызывается после операций и по таймеру окна.
    public void Refresh(int? selectSessionId)
    {
        Occupancy.Clear();
        foreach (var info in _parkingService.GetOccupancy())
        {
            Occupancy.Add(new OccupancyViewModel(info));
        }

        OnPropertyChanged(nameof(RegularFree));
        OnPropertyChanged(nameof(DisabledFree));
        OnPropertyChanged(nameof(ActiveCount));
        OnPropertyChanged(nameof(BookedCount));
        LoadSessions(selectSessionId);
        Bookings.Load();
    }

    private void SetEntryCategory(SpotCategory category)
    {
        if (_entryCategory == category)
        {
            return;
        }

        _entryCategory = category;
        OnPropertyChanged(nameof(EntryCategory));
        OnPropertyChanged(nameof(IsRegularCategory));
        OnPropertyChanged(nameof(IsDisabledCategory));
        CommandManager.InvalidateRequerySuggested();
    }

    private int FreeOf(SpotCategory category)
    {
        return Occupancy.FirstOrDefault(o => o.Category == category)?.Free ?? 0;
    }

    private void LoadSessions(int? selectSessionId)
    {
        var keepId = selectSessionId ?? SelectedSession?.Id;
        var now = _timeProvider.GetLocalNow().DateTime;

        Sessions.Clear();
        foreach (var session in _parkingService.GetActiveSessions(SearchText))
        {
            Sessions.Add(new SessionViewModel(session, now, _parkingService.CalculateAmount(session)));
        }

        SelectedSession = Sessions.FirstOrDefault(s => s.Id == keepId)
            ?? (Sessions.Count == 1 && SearchText.Length > 0 ? Sessions[0] : null);
        CommandManager.InvalidateRequerySuggested();
    }

    private bool CanRegisterEntry()
    {
        // Наличие мест проверяет служба: автомобиль с бронью может заехать,
        // даже когда свободных мест нет, — для него место удержано.
        return !string.IsNullOrWhiteSpace(CarNumber);
    }

    private void RegisterEntry()
    {
        try
        {
            var session = _parkingService.RegisterEntry(CarNumber, _entryCategory);
            StatusMessage = $"Въезд зарегистрирован: {session.CarNumber}, место {session.Category.ToShortName()}.";
            CarNumber = string.Empty;
            Refresh(session.Id);
        }
        catch (ParkingOperationException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private bool CanRegisterExit()
    {
        return SelectedSession is not null;
    }

    private void RegisterExit()
    {
        var selected = SelectedSession;
        if (selected is null)
        {
            return;
        }

        try
        {
            var session = _parkingService.RegisterExit(selected.Id);
            StatusMessage = $"Выезд зарегистрирован: {session.CarNumber}, к оплате {MoneyFormat.Format(session.Amount ?? 0m)}.";
            Refresh(null);
        }
        catch (ParkingOperationException ex)
        {
            StatusMessage = ex.Message;
        }
    }
}
