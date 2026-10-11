using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public class ParkingSpotRepository : IParkingSpotRepository
{
    private readonly ParkingDbContext _dbContext;

    public ParkingSpotRepository(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IReadOnlyList<ParkingSpot> GetAll()
    {
        return _dbContext.ParkingSpots.OrderBy(s => s.Number).ToList();
    }

    public ParkingSpot? GetById(int id)
    {
        return _dbContext.ParkingSpots.Find(id);
    }

    public void Update(ParkingSpot spot)
    {
        ArgumentNullException.ThrowIfNull(spot);

        _dbContext.ParkingSpots.Update(spot);
        _dbContext.SaveChanges();
    }
}
