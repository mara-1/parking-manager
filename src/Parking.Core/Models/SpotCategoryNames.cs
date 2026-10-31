namespace Parking.Core.Models;

public static class SpotCategoryNames
{
    // «Обычные места», «Места для инвалидов» — заголовки на экране.
    public static string ToDisplayName(this SpotCategory category)
    {
        return category switch
        {
            SpotCategory.Regular => "Обычные места",
            SpotCategory.Disabled => "Места для инвалидов",
            _ => category.ToString(),
        };
    }

    // «обычное», «для инвалидов» — короткое название в списках.
    public static string ToShortName(this SpotCategory category)
    {
        return category switch
        {
            SpotCategory.Regular => "обычное",
            SpotCategory.Disabled => "для инвалидов",
            _ => category.ToString(),
        };
    }
}
