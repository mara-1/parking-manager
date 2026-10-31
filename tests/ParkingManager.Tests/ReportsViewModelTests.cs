using System.IO;
using Parking.Core.Models;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;
using ParkingManager.Tests.Fakes;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
[Trait("Task", "10")]
public class ReportsViewModelTests
{
    public const string ExcelPath = @"C:\Отчёты\октябрь.xlsx";

    private readonly ManualTimeProvider _clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0));
    private readonly FakeReportExporter _exporter = new FakeReportExporter();
    private readonly ParkingService _parking;
    private readonly ReportService _reports;

    public ReportsViewModelTests()
    {
        var sessions = new FakeSessionRepository();
        var bookings = new FakeBookingRepository();
        _parking = new ParkingService(new FakeCategoryRepository(regular: 10, disabled: 2), sessions, new FakeTariffRepository(), bookings, _clock);
        _reports = new ReportService(sessions, bookings, _clock);
    }

    [Fact]
    public void Constructor_BuildsReportFromMonthStartToToday()
    {
        var session = _parking.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _clock.Advance(TimeSpan.FromMinutes(45));
        _parking.RegisterExit(session.Id);

        var viewModel = Create(path: null);

        Assert.Equal(new DateTime(2026, 10, 1), viewModel.From);
        Assert.Equal(new DateTime(2026, 10, 24), viewModel.To);
        Assert.Equal("Стоянок: 1 (завершено 1, на парковке 0) · выручка 60 ₽ · средняя стоянка 45 мин", viewModel.TotalsText);
        Assert.Equal("Брони: 0 — заехали 0, неявки 0, отменены 0, ожидаются 0", viewModel.BookingsText);
        Assert.Equal(2, viewModel.Categories.Count);
        Assert.Equal(24, viewModel.Days.Count);
        Assert.True(viewModel.HasReport);
    }

    [Fact]
    public void Build_CategoryWithoutFinishedSessions_ShowsDash()
    {
        _parking.RegisterEntry("М808АА73", SpotCategory.Disabled);

        var viewModel = Create(path: null);

        var disabled = viewModel.Categories.Single(c => c.CategoryName == "Места для инвалидов");
        Assert.Equal(1, disabled.Sessions);
        Assert.Equal("—", disabled.AverageStayText);
        Assert.Equal("0 ₽", disabled.RevenueText);
    }

    [Fact]
    public void Build_EndBeforeStart_ShowsErrorAndDisablesExport()
    {
        var viewModel = Create(path: ExcelPath);
        viewModel.From = new DateTime(2026, 10, 25);

        viewModel.Build();

        Assert.Equal("Дата окончания периода раньше даты начала.", viewModel.Message);
        Assert.False(viewModel.HasReport);
        Assert.False(viewModel.ExportExcelCommand.CanExecute(null));
        Assert.Empty(viewModel.Days);
    }

    [Fact]
    public void ExportExcel_PathChosen_SavesReportAndShowsMessage()
    {
        var dialog = new FakeSaveFileDialog(ExcelPath);
        var viewModel = new ReportsViewModel(_reports, _exporter, dialog, _clock);

        viewModel.ExportExcelCommand.Execute(null);

        Assert.Equal(ExcelPath, _exporter.ExcelPath);
        Assert.Equal("Отчёт_2026-10-01_2026-10-24.xlsx", dialog.LastFileName);
        Assert.Equal(ReportsViewModel.ExcelFilter, dialog.LastFilter);
        Assert.Equal($"Отчёт сохранён: {ExcelPath}", viewModel.Message);
    }

    [Fact]
    public void ExportPdf_DialogCancelled_DoesNotSave()
    {
        var viewModel = Create(path: null);

        viewModel.ExportPdfCommand.Execute(null);

        Assert.Null(_exporter.PdfPath);
        Assert.Equal(string.Empty, viewModel.Message);
    }

    [Fact]
    public void ExportPdf_FileBusy_ShowsError()
    {
        _exporter.ErrorToThrow = new IOException("файл открыт в другой программе");
        var viewModel = Create(path: @"C:\Отчёты\октябрь.pdf");

        viewModel.ExportPdfCommand.Execute(null);

        Assert.Equal("Не удалось сохранить отчёт: файл открыт в другой программе", viewModel.Message);
    }

    [Fact]
    public void ExportExcel_NoAccess_ShowsError()
    {
        _exporter.ErrorToThrow = new UnauthorizedAccessException("доступ запрещён");
        var viewModel = Create(path: ExcelPath);

        viewModel.ExportExcelCommand.Execute(null);

        Assert.Equal("Нет доступа к файлу: доступ запрещён", viewModel.Message);
    }

    [Fact]
    public void Build_NoDates_UsesToday()
    {
        var viewModel = Create(path: null);
        viewModel.From = null;
        viewModel.To = null;

        viewModel.BuildCommand.Execute(null);

        Assert.Single(viewModel.Days);
        Assert.Equal("24.10.2026", viewModel.Days[0].DateText);
    }

    private ReportsViewModel Create(string? path)
    {
        return new ReportsViewModel(_reports, _exporter, new FakeSaveFileDialog(path), _clock);
    }
}
