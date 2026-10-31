using System.Text;
using System.Text.RegularExpressions;

namespace Parking.Core.Services;

// Приведение и проверка государственного номера автомобиля (формат РФ: А123ВС73, А123ВС173).
public static partial class CarNumberFormat
{
    public const string Example = "А123ВС73";

    // Латинские буквы, совпадающие по начертанию с русскими, и их русские пары
    // (на номерах РФ используются только эти 12 букв).
    private static string LatinLetters => "ABEKMHOPCTYX";

    private static string CyrillicLetters => "АВЕКМНОРСТУХ";

    // Убирает пробелы и дефисы, переводит в верхний регистр и заменяет латинские
    // буквы русскими (A → А, B → В и т. д.).
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value.ToUpperInvariant())
        {
            if (char.IsWhiteSpace(ch) || ch == '-')
            {
                continue;
            }

            builder.Append(ToCyrillic(ch));
        }

        return builder.ToString();
    }

    public static bool IsValid(string? normalized)
    {
        return PlatePattern().IsMatch(normalized ?? string.Empty);
    }

    // Приводит номер к единому виду и проверяет формат. Используется при въезде
    // и при бронировании, чтобы правила и тексты ошибок были одинаковыми.
    public static string RequireValid(string? carNumber)
    {
        var normalized = Normalize(carNumber);
        if (normalized.Length == 0)
        {
            throw new ParkingOperationException("Не указан номер автомобиля.");
        }

        if (!IsValid(normalized))
        {
            throw new ParkingOperationException($"Номер автомобиля указан неверно. Пример: {Example}.");
        }

        return normalized;
    }

    private static char ToCyrillic(char ch)
    {
        var index = LatinLetters.IndexOf(ch, StringComparison.Ordinal);
        return index < 0 ? ch : CyrillicLetters[index];
    }

    [GeneratedRegex("^[АВЕКМНОРСТУХ][0-9]{3}[АВЕКМНОРСТУХ]{2}[0-9]{2,3}$")]
    private static partial Regex PlatePattern();
}
