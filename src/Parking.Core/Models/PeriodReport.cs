namespace Parking.Core.Models;

// Отчёт о работе парковки за период [From, To] (обе даты включительно).
// Стоянка относится к периоду по дате въезда; выручка — сумма, оплаченная при выезде.
public sealed class PeriodReport
{
    public DateTime From { get; init; }

    public DateTime To { get; init; }

    public DateTime CreatedAt { get; init; }

    public int TotalSessions { get; init; }

    public int FinishedSessions { get; init; }

    // Стоянки периода, которые на момент отчёта ещё не завершены.
    public int ActiveSessions { get; init; }

    public decimal Revenue { get; init; }

    public TimeSpan AverageStay { get; init; }

    public int BookingsTotal { get; init; }

    public int BookingsArrived { get; init; }

    public int BookingsNoShow { get; init; }

    public int BookingsCancelled { get; init; }

    public int BookingsWaiting { get; init; }

    public IReadOnlyList<ReportCategoryRow> Categories { get; init; } = Array.Empty<ReportCategoryRow>();

    public IReadOnlyList<ReportDayRow> Days { get; init; } = Array.Empty<ReportDayRow>();
}
