using Parking.Core.Models;

namespace ParkingManager.ViewModels;

// Строка таблицы «По категориям мест» на вкладке «Отчёты».
public sealed class ReportCategoryViewModel
{
    public ReportCategoryViewModel(ReportCategoryRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        CategoryName = row.Category.ToDisplayName();
        Sessions = row.Sessions;
        Finished = row.Finished;
        RevenueText = MoneyFormat.Format(row.Revenue);
        AverageStayText = ReportsViewModel.StayText(row.Finished, row.AverageStay);
    }

    public string CategoryName { get; }

    public int Sessions { get; }

    public int Finished { get; }

    public string RevenueText { get; }

    public string AverageStayText { get; }
}
