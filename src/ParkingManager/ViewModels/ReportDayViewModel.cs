using System.Globalization;
using Parking.Core.Models;

namespace ParkingManager.ViewModels;

// Строка таблицы «По дням» на вкладке «Отчёты».
public sealed class ReportDayViewModel
{
    public ReportDayViewModel(ReportDayRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        DateText = row.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        Entries = row.Entries;
        RevenueText = MoneyFormat.Format(row.Revenue);
    }

    public string DateText { get; }

    public int Entries { get; }

    public string RevenueText { get; }
}
