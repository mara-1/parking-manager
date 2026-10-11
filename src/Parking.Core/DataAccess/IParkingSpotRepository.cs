using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public interface IParkingSpotRepository
{
    IReadOnlyList<ParkingSpot> GetAll();

    ParkingSpot? GetById(int id);

    void Update(ParkingSpot spot);
}
