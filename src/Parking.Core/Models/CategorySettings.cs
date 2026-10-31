namespace Parking.Core.Models;

// Настройки категории мест для экрана «Тарифы и места».
public sealed class CategorySettings
{
    public SpotCategory Category { get; init; }

    public int Capacity { get; init; }

    public decimal PricePerHour { get; init; }

    // Сколько мест занято или удерживается бронями: ниже этого вместимость не уменьшить.
    public int InUse { get; init; }
}
