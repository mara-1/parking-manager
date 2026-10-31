using System.Text;
using System.Text.RegularExpressions;

namespace Parking.Core.Services;

// Приведение и проверка государственного номера автомобиля (формат РФ: А123ВС73, А123ВС173).
public static partial class CarNumberFormat
{
    public const string Example = "А123ВС73";

    // Убирает пробелы и дефисы, переводит в верхний регистр и заменяет латинские
    // буквы, совпадающие по начертанию с русскими (A, B, E, K, M, H, O, P, C, T, Y, X).
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

    public static bool IsValid(string normalized)
    {
        return PlatePattern().IsMatch(normalized ?? string.Empty);
    }

    private static char ToCyrillic(char ch)
    {
        return ch switch
        {
            'A' => 'А',
            'B' => 'В',
            'E' => 'Е',
            'K' => 'К',
            'M' => 'М',
            'H' => 'Н',
            'O' => 'О',
            'P' => 'Р',
            'C' => 'С',
            'T' => 'Т',
            'Y' => 'У',
            'X' => 'Х',
            _ => ch,
        };
    }

    [GeneratedRegex("^[АВЕКМНОРСТУХ][0-9]{3}[АВЕКМНОРСТУХ]{2}[0-9]{2,3}$")]
    private static partial Regex PlatePattern();
}
