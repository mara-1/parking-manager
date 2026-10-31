using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public interface ITariffRepository
{
    IReadOnlyList<Tariff> GetAll();

    Tariff? Find(SpotCategory category);

    void Update(Tariff tariff);
}
