namespace Parking.Core.Models;

// Строка отчёта за один день периода.
public sealed class ReportDayRow
{
    public DateTime Date { get; init; }

    public int Entries { get; init; }

    public decimal Revenue { get; init; }
}
