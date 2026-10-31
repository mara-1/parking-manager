using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Tests.Fakes;

public class FakeTariffRepository : ITariffRepository
{
    private readonly List<Tariff> _tariffs = new List<Tariff>();

    public FakeTariffRepository(decimal regular = 60m, decimal disabled = 0m)
    {
        _tariffs.Add(new Tariff { Category = SpotCategory.Regular, PricePerHour = regular });
        _tariffs.Add(new Tariff { Category = SpotCategory.Disabled, PricePerHour = disabled });
    }

    public IReadOnlyList<Tariff> GetAll()
    {
        return _tariffs;
    }

    public Tariff? Find(SpotCategory category)
    {
        return _tariffs.Find(t => t.Category == category);
    }

    public void Update(Tariff tariff)
    {
        // Объекты хранятся по ссылке, изменения уже применены.
    }
}
