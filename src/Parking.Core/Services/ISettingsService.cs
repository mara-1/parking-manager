using Parking.Core.Models;

namespace Parking.Core.Services;

public interface ISettingsService
{
    IReadOnlyList<CategorySettings> GetSettings();

    // Обе вместимости меняются вместе: доля мест для инвалидов зависит от общего числа.
    void UpdateCapacities(int regularCapacity, int disabledCapacity);

    void UpdatePrice(SpotCategory category, decimal pricePerHour);
}
