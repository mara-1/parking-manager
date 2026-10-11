using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace ParkingManager.Tests.Fakes;

// Хранилище мест в памяти: позволяет проверять модели представления
// вместе с настоящей службой учёта мест без базы данных.
public sealed class InMemorySpotRepository : IParkingSpotRepository
{
    private readonly List<ParkingSpot> _spots = new List<ParkingSpot>();

    public InMemorySpotRepository(int count)
    {
        for (var i = 1; i <= count; i++)
        {
            _spots.Add(new ParkingSpot { Id = i, Number = $"A-{i:D2}", Status = SpotStatus.Free });
        }
    }

    public IReadOnlyList<ParkingSpot> GetAll()
    {
        return _spots;
    }

    public ParkingSpot? GetById(int id)
    {
        return _spots.Find(s => s.Id == id);
    }

    public void Update(ParkingSpot spot)
    {
        // Объекты хранятся по ссылке, поэтому изменения уже применены.
    }
}
