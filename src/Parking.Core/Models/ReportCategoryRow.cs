namespace Parking.Core.Models;

// Строка отчёта по категории мест.
public sealed class ReportCategoryRow
{
    public SpotCategory Category { get; init; }

    // Стоянки с въездом в периоде (завершённые и ещё идущие).
    public int Sessions { get; init; }

    public int Finished { get; init; }

    // Оплачено при выезде по завершённым стоянкам.
    public decimal Revenue { get; init; }

    public TimeSpan AverageStay { get; init; }
}
