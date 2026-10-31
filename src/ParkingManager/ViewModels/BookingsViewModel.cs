using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Parking.Core.Models;
using Parking.Core.Services;

namespace ParkingManager.ViewModels;

// Вкладка «Брони»: брони на сегодня, создание и отмена брони.
public sealed class BookingsViewModel : ViewModelBase
{
    private readonly IBookingService _bookingService;
    private readonly TimeProvider _timeProvider;
    private SpotCategory _category = SpotCategory.Regular;
    private BookingViewModel? _selectedBooking;
    private string _carNumber = string.Empty;
    private string _arrivalText;
    private string _message = string.Empty;

    public BookingsViewModel(IBookingService bookingService, TimeProvider timeProvider)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        _arrivalText = DefaultArrival();
        Bookings = new ObservableCollection<BookingViewModel>();
        CreateCommand = new RelayCommand(Create, () => !string.IsNullOrWhiteSpace(CarNumber));
        CancelCommand = new RelayCommand(Cancel, () => SelectedBooking is { IsWaiting: true });
        Load();
    }

    public event EventHandler? Changed;

    public ObservableCollection<BookingViewModel> Bookings { get; }

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

    public bool IsRegularCategory
    {
        get => _category == SpotCategory.Regular;
        set
        {
            if (value)
            {
                SetCategory(SpotCategory.Regular);
            }
        }
    }

    public bool IsDisabledCategory
    {
        get => _category == SpotCategory.Disabled;
        set
        {
            if (value)
            {
                SetCategory(SpotCategory.Disabled);
            }
        }
    }

    // Время прибытия в формате ЧЧ:ММ, например 18:30.
    public string ArrivalText
    {
        get => _arrivalText;
        set => SetProperty(ref _arrivalText, value ?? string.Empty);
    }

    public BookingViewModel? SelectedBooking
    {
        get => _selectedBooking;
        set
        {
            if (SetProperty(ref _selectedBooking, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public ICommand CreateCommand { get; }

    public ICommand CancelCommand { get; }

    public void Load()
    {
        var selectedId = SelectedBooking?.Id;
        Bookings.Clear();
        foreach (var booking in _bookingService.GetBookingsForDay(Today()))
        {
            Bookings.Add(new BookingViewModel(booking));
        }

        SelectedBooking = Bookings.FirstOrDefault(b => b.Id == selectedId);
    }

    private void SetCategory(SpotCategory category)
    {
        if (_category == category)
        {
            return;
        }

        _category = category;
        OnPropertyChanged(nameof(IsRegularCategory));
        OnPropertyChanged(nameof(IsDisabledCategory));
    }

    private void Create()
    {
        if (!TimeOnly.TryParseExact(ArrivalText.Trim(), "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
        {
            Message = "Время прибытия укажите в формате ЧЧ:ММ, например 18:30.";
            return;
        }

        try
        {
            var booking = _bookingService.CreateBooking(CarNumber, _category, Today().Add(time.ToTimeSpan()));
            Message = $"Бронь создана: {booking.CarNumber} на {booking.ArrivalTime.ToString("HH:mm", CultureInfo.InvariantCulture)}.";
            CarNumber = string.Empty;
            Load();
            Changed?.Invoke(this, EventArgs.Empty);
        }
        catch (ParkingOperationException ex)
        {
            Message = ex.Message;
        }
    }

    private void Cancel()
    {
        var selected = SelectedBooking;
        if (selected is null)
        {
            return;
        }

        try
        {
            _bookingService.CancelBooking(selected.Id);
            Message = $"Бронь отменена: {selected.CarNumber}.";
            Load();
            Changed?.Invoke(this, EventArgs.Empty);
        }
        catch (ParkingOperationException ex)
        {
            Message = ex.Message;
        }
    }

    private DateTime Today()
    {
        return _timeProvider.GetLocalNow().DateTime.Date;
    }

    // По умолчанию — через 30 минут, но не позже конца дня.
    private string DefaultArrival()
    {
        var now = _timeProvider.GetLocalNow().DateTime;
        var arrival = now.AddMinutes(30);
        if (arrival.Date != now.Date)
        {
            arrival = now.Date.AddHours(23).AddMinutes(59);
        }

        return arrival.ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
