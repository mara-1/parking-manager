namespace Parking.Core.Services;

// Расчёт оплаты: оплачивается каждый начатый час, минимум один час.
public static class TariffCalculator
{
    public static int BillableHours(TimeSpan duration)
    {
        var minutes = Math.Max(1, (int)Math.Ceiling(duration.TotalMinutes));
        return (minutes + 59) / 60;
    }

    public static decimal Calculate(TimeSpan duration, decimal pricePerHour)
    {
        return BillableHours(duration) * pricePerHour;
    }
}
