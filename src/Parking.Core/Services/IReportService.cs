using Parking.Core.Models;

namespace Parking.Core.Services;

public interface IReportService
{
    // Отчёт за период с fromDay по toDay включительно (время в датах не учитывается).
    PeriodReport Build(DateTime fromDay, DateTime toDay);
}
