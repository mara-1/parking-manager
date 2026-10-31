namespace Parking.Core.Services;

public static class ParkingClock
{
    // Текущее время с точностью до минуты: секунды оператору и в отчётах не нужны.
    public static DateTime Now(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        var now = timeProvider.GetLocalNow().DateTime;
        return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
    }
}
