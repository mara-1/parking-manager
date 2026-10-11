using Parking.Core.Models;

namespace Parking.Core.Services;

public interface IParkingService
{
    IReadOnlyList<ParkingSpot> GetSpots();

    void RegisterEntry(int spotId, string carNumber);

    void RegisterExit(int spotId);
}
