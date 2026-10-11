using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Tests.Fakes;

public class FakeParkingSpotRepository : IParkingSpotRepository
{
    private readonly List<ParkingSpot> _spots = new List<ParkingSpot>();

    public FakeParkingSpotRepository(int count)
    {
        for (var i = 1; i <= count; i++)
        {
            _spots.Add(new ParkingSpot { Id = i, Number = $"A-{i:D2}", Status = SpotStatus.Free });
        }
    }

    public int UpdateCount { get; private set; }

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
        UpdateCount++;
    }
}
