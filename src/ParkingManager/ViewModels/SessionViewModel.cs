using System.Globalization;
using Parking.Core.Models;

namespace ParkingManager.ViewModels;

// Строка списка стоянок.
public sealed class SessionViewModel
{
    public SessionViewModel(ParkingSession session, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(session);

        Id = session.Id;
        CarNumber = session.CarNumber;
        Category = session.Category;
        CategoryName = session.Category.ToShortName();
        EntryTimeText = session.EntryTime.ToString("dd.MM.yyyy HH:mm", Russian);
        ExitTimeText = session.ExitTime?.ToString("dd.MM.yyyy HH:mm", Russian) ?? "на парковке";
        DurationText = DurationFormat.Format((session.ExitTime ?? now) - session.EntryTime);
    }

    public int Id { get; }

    public string CarNumber { get; }

    public SpotCategory Category { get; }

    public string CategoryName { get; }

    public string EntryTimeText { get; }

    public string ExitTimeText { get; }

    public string DurationText { get; }

    private static CultureInfo Russian => CultureInfo.GetCultureInfo("ru-RU");
}
