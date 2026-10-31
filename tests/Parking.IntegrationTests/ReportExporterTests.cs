using ClosedXML.Excel;
using Parking.Core.Models;
using Parking.Core.Services;

namespace Parking.IntegrationTests;

// Выгрузка отчёта в настоящие файлы: Excel читается обратно через ClosedXML,
// у PDF проверяется сигнатура формата.
[Trait("Category", "Integration")]
[Trait("Task", "10")]
public sealed class ReportExporterTests : IDisposable
{
    private readonly ReportExporter _exporter = new ReportExporter();
    private readonly List<string> _files = new List<string>();

    [Fact]
    public void ExportToExcel_WritesSummaryCategoriesAndDays()
    {
        var path = TempFile("xlsx");

        _exporter.ExportToExcel(SampleReport(), path);

        using var workbook = new XLWorkbook(path);
        Assert.Equal(
            new List<string> { ReportExporter.SummarySheet, ReportExporter.CategoriesSheet, ReportExporter.DaysSheet },
            workbook.Worksheets.Select(w => w.Name).ToList());

        var summary = workbook.Worksheet(ReportExporter.SummarySheet);
        Assert.Equal("Отчёт о работе парковки за 24.10.2026 - 25.10.2026", summary.Cell(1, 1).GetString());
        Assert.Equal("Стоянок за период", summary.Cell(5, 1).GetString());
        Assert.Equal(3, summary.Cell(5, 2).GetDouble());

        var categories = workbook.Worksheet(ReportExporter.CategoriesSheet);
        Assert.Equal("Обычные места", categories.Cell(2, 1).GetString());
        Assert.Equal(1260, categories.Cell(2, 4).GetDouble());
        Assert.Equal("Места для инвалидов", categories.Cell(3, 1).GetString());
        Assert.Equal("Итого", categories.Cell(4, 1).GetString());
        Assert.Equal(3, categories.Cell(4, 2).GetDouble());

        var days = workbook.Worksheet(ReportExporter.DaysSheet);
        Assert.Equal(new DateTime(2026, 10, 24), days.Cell(2, 1).GetDateTime());
        Assert.Equal(2, days.Cell(2, 2).GetDouble());
        Assert.Equal(new DateTime(2026, 10, 25), days.Cell(3, 1).GetDateTime());
    }

    [Fact]
    public void ExportToPdf_CreatesPdfDocument()
    {
        var path = TempFile("pdf");

        _exporter.ExportToPdf(SampleReport(), path);

        var bytes = File.ReadAllBytes(path);
        Assert.True(bytes.Length > 1000, $"Слишком маленький файл: {bytes.Length} байт.");
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void ExportToExcel_FileIsBusy_ThrowsIOException()
    {
        var path = TempFile("xlsx");
        using var busy = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

        Assert.ThrowsAny<IOException>(() => _exporter.ExportToExcel(SampleReport(), path));
    }

    public void Dispose()
    {
        foreach (var file in _files.Where(File.Exists))
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void ExportToExcel_EmptyDay_WritesZerosAndSingleDate()
    {
        var path = TempFile("xlsx");

        _exporter.ExportToExcel(EmptyReport(), path);

        using var workbook = new XLWorkbook(path);
        Assert.Equal("Отчёт о работе парковки за 24.10.2026", workbook.Worksheet(ReportExporter.SummarySheet).Cell(1, 1).GetString());
        var categories = workbook.Worksheet(ReportExporter.CategoriesSheet);
        Assert.Equal("Итого", categories.Cell(2, 1).GetString());
        Assert.Equal(0, categories.Cell(2, 2).GetDouble());
    }

    [Fact]
    public void ExportToPdf_EmptyDay_CreatesPdfDocument()
    {
        var path = TempFile("pdf");

        _exporter.ExportToPdf(EmptyReport(), path);

        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(path), 0, 4));
    }

    // Отчёт за день без стоянок: таблицы категорий и дней пустые.
    private static PeriodReport EmptyReport()
    {
        return new PeriodReport
        {
            From = new DateTime(2026, 10, 24),
            To = new DateTime(2026, 10, 24),
            CreatedAt = new DateTime(2026, 10, 24, 20, 0, 0),
        };
    }

    private static PeriodReport SampleReport()
    {
        return new PeriodReport
        {
            From = new DateTime(2026, 10, 24),
            To = new DateTime(2026, 10, 25),
            CreatedAt = new DateTime(2026, 10, 25, 18, 30, 0),
            TotalSessions = 3,
            FinishedSessions = 2,
            ActiveSessions = 1,
            Revenue = 1260m,
            AverageStay = TimeSpan.FromMinutes(70),
            BookingsTotal = 2,
            BookingsArrived = 1,
            BookingsNoShow = 1,
            Categories = new List<ReportCategoryRow>
            {
                new ReportCategoryRow { Category = SpotCategory.Regular, Sessions = 2, Finished = 2, Revenue = 1260m, AverageStay = TimeSpan.FromMinutes(70) },
                new ReportCategoryRow { Category = SpotCategory.Disabled, Sessions = 1, Finished = 0, Revenue = 0m, AverageStay = TimeSpan.Zero },
            },
            Days = new List<ReportDayRow>
            {
                new ReportDayRow { Date = new DateTime(2026, 10, 24), Entries = 2, Revenue = 1260m },
                new ReportDayRow { Date = new DateTime(2026, 10, 25), Entries = 1, Revenue = 0m },
            },
        };
    }

    private string TempFile(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"parking-report-{Guid.NewGuid():N}.{extension}");
        _files.Add(path);
        return path;
    }
}
