using System.Collections.ObjectModel;
using System.Windows.Input;
using Parking.Core.Services;

namespace ParkingManager.ViewModels;

// Вкладка «История»: стоянки за период с поиском по номеру.
public sealed class HistoryViewModel : ViewModelBase
{
    private readonly IParkingService _parkingService;
    private readonly TimeProvider _timeProvider;
    private DateTime? _from;
    private DateTime? _to;
    private string _searchText = string.Empty;
    private string _summary = string.Empty;

    public HistoryViewModel(IParkingService parkingService, TimeProvider timeProvider)
    {
        _parkingService = parkingService ?? throw new ArgumentNullException(nameof(parkingService));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        var today = Today();
        _from = today;
        _to = today;
        Sessions = new ObservableCollection<SessionViewModel>();
        ShowCommand = new RelayCommand(Show);
        Show();
    }

    public DateTime? From
    {
        get => _from;
        set => SetProperty(ref _from, value);
    }

    public DateTime? To
    {
        get => _to;
        set => SetProperty(ref _to, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value ?? string.Empty);
    }

    public string Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    public ObservableCollection<SessionViewModel> Sessions { get; }

    public ICommand ShowCommand { get; }

    public void Show()
    {
        var from = (From ?? Today()).Date;
        var to = (To ?? Today()).Date.AddDays(1);
        Sessions.Clear();

        if (to <= from)
        {
            Summary = "Дата окончания периода раньше даты начала.";
            return;
        }

        var now = _timeProvider.GetLocalNow().DateTime;
        foreach (var session in _parkingService.GetHistory(from, to, SearchText))
        {
            Sessions.Add(new SessionViewModel(session, now, _parkingService.CalculateAmount(session)));
        }

        Summary = $"Найдено стоянок: {Sessions.Count} · сумма {MoneyFormat.Format(Sessions.Sum(s => s.Amount))}";
    }

    private DateTime Today()
    {
        return _timeProvider.GetLocalNow().DateTime.Date;
    }
}
