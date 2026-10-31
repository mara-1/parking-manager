using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public class TariffRepository : ITariffRepository
{
    private readonly ParkingDbContext _dbContext;

    public TariffRepository(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IReadOnlyList<Tariff> GetAll()
    {
        return _dbContext.Tariffs.AsEnumerable().OrderBy(t => t.Category).ToList();
    }

    public Tariff? Find(SpotCategory category)
    {
        return _dbContext.Tariffs.Find(category);
    }

    public void Update(Tariff tariff)
    {
        ArgumentNullException.ThrowIfNull(tariff);

        _dbContext.Tariffs.Update(tariff);
        _dbContext.SaveChanges();
    }
}
