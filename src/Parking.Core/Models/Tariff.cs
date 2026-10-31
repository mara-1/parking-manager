namespace Parking.Core.Models;

// Тариф категории мест: стоимость каждого начатого часа стоянки.
// Для мест для инвалидов по умолчанию 0 ₽ (бесплатная парковка, 181-ФЗ, ст. 15).
public class Tariff
{
    public SpotCategory Category { get; set; }

    public decimal PricePerHour { get; set; }
}
