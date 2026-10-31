using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Services;

// Вместимость парковки и тарифы. Правила:
// вместимость нельзя сделать меньше числа занятых мест;
// мест для инвалидов — не менее 10 % от общего числа и не менее одного (181-ФЗ, ст. 15);
// стоимость часа — от 0 до 10 000 ₽.
public class SettingsService : ISettingsService
{
    public const decimal MaxPricePerHour = 10000m;

    private readonly ICategoryRepository _categories;
    private readonly ITariffRepository _tariffs;
    private readonly IParkingService _parking;

    public SettingsService(ICategoryRepository categories, ITariffRepository tariffs, IParkingService parking)
    {
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        _tariffs = tariffs ?? throw new ArgumentNullException(nameof(tariffs));
        _parking = parking ?? throw new ArgumentNullException(nameof(parking));
    }

    // 10 % от общего числа мест с округлением вверх, но не меньше одного места.
    public static int MinimumDisabledPlaces(int totalPlaces)
    {
        return Math.Max(1, (totalPlaces + 9) / 10);
    }

    public IReadOnlyList<CategorySettings> GetSettings()
    {
        var occupancy = _parking.GetOccupancy();
        return _categories.GetAll()
            .Select(c =>
            {
                var used = occupancy.FirstOrDefault(o => o.Category == c.Category);
                return new CategorySettings
                {
                    Category = c.Category,
                    Capacity = c.Capacity,
                    PricePerHour = _tariffs.Find(c.Category)?.PricePerHour ?? 0m,
                    InUse = used is null ? 0 : used.Occupied + used.Booked,
                };
            })
            .ToList();
    }

    public void UpdateCapacities(int regularCapacity, int disabledCapacity)
    {
        if (regularCapacity < 0 || disabledCapacity < 0)
        {
            throw new ParkingOperationException("Вместимость не может быть отрицательной.");
        }

        var settings = GetSettings();
        CheckNotBelowInUse(settings, SpotCategory.Regular, regularCapacity);
        CheckNotBelowInUse(settings, SpotCategory.Disabled, disabledCapacity);

        var minimumDisabled = MinimumDisabledPlaces(regularCapacity + disabledCapacity);
        if (disabledCapacity < minimumDisabled)
        {
            throw new ParkingOperationException(
                $"Мест для инвалидов должно быть не менее 10 % от общего числа: не менее {minimumDisabled}.");
        }

        Save(SpotCategory.Regular, regularCapacity);
        Save(SpotCategory.Disabled, disabledCapacity);
    }

    public void UpdatePrice(SpotCategory category, decimal pricePerHour)
    {
        if (pricePerHour < 0 || pricePerHour > MaxPricePerHour)
        {
            throw new ParkingOperationException($"Стоимость часа должна быть от 0 до {MaxPricePerHour:0} ₽.");
        }

        var tariff = _tariffs.Find(category)
            ?? throw new ParkingOperationException("Тариф категории не настроен.");
        tariff.PricePerHour = pricePerHour;
        _tariffs.Update(tariff);
    }

    private static void CheckNotBelowInUse(IReadOnlyList<CategorySettings> settings, SpotCategory category, int capacity)
    {
        var inUse = settings.FirstOrDefault(s => s.Category == category)?.InUse ?? 0;
        if (capacity < inUse)
        {
            throw new ParkingOperationException(
                $"{category.ToDisplayName()}: нельзя сделать мест меньше, чем сейчас занято ({inUse}).");
        }
    }

    private void Save(SpotCategory category, int capacity)
    {
        var current = _categories.Find(category)
            ?? throw new ParkingOperationException("Категория мест не настроена.");
        if (current.Capacity != capacity)
        {
            current.Capacity = capacity;
            _categories.Update(current);
        }
    }
}
