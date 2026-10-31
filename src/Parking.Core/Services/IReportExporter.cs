using Parking.Core.Models;

namespace Parking.Core.Services;

public interface IReportExporter
{
    void ExportToExcel(PeriodReport report, string path);

    void ExportToPdf(PeriodReport report, string path);
}
