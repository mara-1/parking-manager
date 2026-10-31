using System.Globalization;

namespace ParkingManager.ViewModels;

public static class MoneyFormat
{
    // 60 ₽ · 1 200 ₽ (разделитель разрядов — обычный пробел, независимо от настроек Windows)
    public static string Format(decimal amount)
    {
        return amount.ToString("#,0.##", Rubles) + " ₽";
    }

    private static NumberFormatInfo Rubles => new NumberFormatInfo { NumberGroupSeparator = " ", NumberDecimalSeparator = "," };
}
