using System.Globalization;
using Parking.Core.Models;

namespace Parking.Core.Services;

// Текстовое представление значений отчёта для выгрузки в PDF.
public static class ReportText
{
    private static CultureInfo Russian => CultureInfo.GetCultureInfo("ru-RU");

    private static NumberFormatInfo Rubles => new NumberFormatInfo { NumberGroupSeparator = " ", NumberDecimalSeparator = "," };

    public static string Day(DateTime day)
    {
        return day.ToString("dd.MM.yyyy", Russian);
    }

    public static string Period(PeriodReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return report.From == report.To ? Day(report.From) : $"{Day(report.From)} - {Day(report.To)}";
    }

    // 1 200 руб. (знак рубля заменён сокращением: он есть не во всех шрифтах PDF)
    public static string Money(decimal amount)
    {
        return amount.ToString("#,0.##", Rubles) + " руб.";
    }

    // 45 мин · 1 ч 35 мин · 26 ч 10 мин
    public static string Duration(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            return "-";
        }

        var hours = (int)duration.TotalHours;
        return hours == 0 ? $"{duration.Minutes} мин" : $"{hours} ч {duration.Minutes} мин";
    }
}
