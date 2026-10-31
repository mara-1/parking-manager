using Parking.Core.Models;
using Parking.Core.Services;

namespace ParkingManager.Tests.Fakes;

// Запоминает, куда «сохранялся» отчёт; может имитировать ошибку записи.
public sealed class FakeReportExporter : IReportExporter
{
    public string? ExcelPath { get; private set; }

    public string? PdfPath { get; private set; }

    public Exception? ErrorToThrow { get; set; }

    public void ExportToExcel(PeriodReport report, string path)
    {
        ThrowIfNeeded();
        ExcelPath = path;
    }

    public void ExportToPdf(PeriodReport report, string path)
    {
        ThrowIfNeeded();
        PdfPath = path;
    }

    private void ThrowIfNeeded()
    {
        if (ErrorToThrow is not null)
        {
            throw ErrorToThrow;
        }
    }
}
