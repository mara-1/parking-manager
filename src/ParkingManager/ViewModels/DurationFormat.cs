namespace ParkingManager.ViewModels;

public static class DurationFormat
{
    // 45 мин · 3 ч 22 мин · 2 д 5 ч
    public static string Format(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            duration = TimeSpan.Zero;
        }

        if (duration.TotalHours < 1)
        {
            return $"{duration.Minutes} мин";
        }

        if (duration.TotalDays < 1)
        {
            return $"{duration.Hours} ч {duration.Minutes} мин";
        }

        return $"{(int)duration.TotalDays} д {duration.Hours} ч";
    }
}
