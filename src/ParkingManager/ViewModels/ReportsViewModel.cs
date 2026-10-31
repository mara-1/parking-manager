using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using Parking.Core.Models;
using Parking.Core.Services;
using ParkingManager.Services;

namespace ParkingManager.ViewModels;

// Вкладка «Отчёты»: статистика за период и выгрузка в Excel и PDF.
public sealed class ReportsViewModel : ViewModelBase
{
    public const string ExcelFilter = "Книга Excel (*.xlsx)|*.xlsx";
    public const string PdfFilter = "Документ PDF (*.pdf)|*.pdf";

    private readonly IReportService _reportService;
    private readonly IReportExporter _exporter;
    private readonly ISaveFileDialog _saveFileDialog;
    private readonly TimeProvider _timeProvider;
    private PeriodReport? _report;
    private DateTime? _from;
    private DateTime? _to;
    private string _totalsText = string.Empty;
    private string _bookingsText = string.Empty;
    private string _message = string.Empty;

    public ReportsViewModel(
        IReportService reportService,
        IReportExporter exporter,
        ISaveFileDialog saveFileDialog,
        TimeProvider timeProvider)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
        _saveFileDialog = saveFileDialog ?? throw new ArgumentNullException(nameof(saveFileDialog));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        // По умолчанию — с начала текущего месяца по сегодня.
        var today = Today();
        _from = new DateTime(today.Year, today.Month, 1);
        _to = today;
        Categories = new ObservableCollection<ReportCategoryViewModel>();
        Days = new ObservableCollection<ReportDayViewModel>();
        BuildCommand = new RelayCommand(Build);
        ExportExcelCommand = new RelayCommand(ExportExcel, () => _report is not null);
        ExportPdfCommand = new RelayCommand(ExportPdf, () => _report is not null);
        Build();
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

    public ObservableCollection<ReportCategoryViewModel> Categories { get; }

    public ObservableCollection<ReportDayViewModel> Days { get; }

    public string TotalsText
    {
        get => _totalsText;
        private set => SetProperty(ref _totalsText, value);
    }

    public string BookingsText
    {
        get => _bookingsText;
        private set => SetProperty(ref _bookingsText, value);
    }

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public bool HasReport => _report is not null;

    public ICommand BuildCommand { get; }

    public ICommand ExportExcelCommand { get; }

    public ICommand ExportPdfCommand { get; }

    // Имя файла по умолчанию: «Отчёт_2026-11-01_2026-11-30.xlsx».
    public static string DefaultFileName(PeriodReport report, string extension)
    {
        ArgumentNullException.ThrowIfNull(report);

        var from = report.From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var to = report.To.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return $"Отчёт_{from}_{to}.{extension}";
    }

    // Средняя длительность; если завершённых стоянок нет — прочерк.
    public static string StayText(int finished, TimeSpan averageStay)
    {
        return finished == 0 ? "—" : DurationFormat.Format(averageStay);
    }

    public void Build()
    {
        Categories.Clear();
        Days.Clear();

        PeriodReport report;
        try
        {
            report = _reportService.Build(From ?? Today(), To ?? Today());
        }
        catch (ParkingOperationException ex)
        {
            SetReport(null);
            TotalsText = string.Empty;
            BookingsText = string.Empty;
            Message = ex.Message;
            return;
        }

        foreach (var row in report.Categories)
        {
            Categories.Add(new ReportCategoryViewModel(row));
        }

        foreach (var row in report.Days)
        {
            Days.Add(new ReportDayViewModel(row));
        }

        TotalsText = $"Стоянок: {report.TotalSessions} (завершено {report.FinishedSessions}, на парковке {report.ActiveSessions})"
            + $" · выручка {MoneyFormat.Format(report.Revenue)}"
            + $" · средняя стоянка {StayText(report.FinishedSessions, report.AverageStay)}";
        BookingsText = $"Брони: {report.BookingsTotal} — заехали {report.BookingsArrived}, неявки {report.BookingsNoShow},"
            + $" отменены {report.BookingsCancelled}, ожидаются {report.BookingsWaiting}";
        Message = string.Empty;
        SetReport(report);
    }

    private void SetReport(PeriodReport? report)
    {
        _report = report;
        OnPropertyChanged(nameof(HasReport));
        CommandManager.InvalidateRequerySuggested();
    }

    private void ExportExcel()
    {
        Export("xlsx", ExcelFilter, _exporter.ExportToExcel);
    }

    private void ExportPdf()
    {
        Export("pdf", PdfFilter, _exporter.ExportToPdf);
    }

    private void Export(string extension, string filter, Action<PeriodReport, string> export)
    {
        var report = _report;
        if (report is null)
        {
            return;
        }

        var path = _saveFileDialog.AskPath(DefaultFileName(report, extension), filter);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            export(report, path);
            Message = $"Отчёт сохранён: {path}";
        }
        catch (IOException ex)
        {
            // Чаще всего файл открыт в Excel или в программе просмотра PDF.
            Message = $"Не удалось сохранить отчёт: {ex.Message}";
        }
        catch (UnauthorizedAccessException ex)
        {
            Message = $"Нет доступа к файлу: {ex.Message}";
        }
    }

    private DateTime Today()
    {
        return _timeProvider.GetLocalNow().DateTime.Date;
    }
}
