using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Services;

public class ParkingService : IParkingService
{
    private readonly IParkingSpotRepository _spots;

    public ParkingService(IParkingSpotRepository spots)
    {
        _spots = spots ?? throw new ArgumentNullException(nameof(spots));
    }

    public IReadOnlyList<ParkingSpot> GetSpots()
    {
        return _spots.GetAll();
    }

    public void RegisterEntry(int spotId, string carNumber)
    {
        if (string.IsNullOrWhiteSpace(carNumber))
        {
            throw new ArgumentException("Не указан номер автомобиля.", nameof(carNumber));
        }

        var normalized = carNumber.Trim().ToUpperInvariant();
        var spot = _spots.GetById(spotId) ?? throw new InvalidOperationException("Парковочное место не найдено.");

        if (spot.Status != SpotStatus.Free)
        {
            throw new InvalidOperationException($"Место {spot.Number} не свободно.");
        }

        if (_spots.GetAll().Any(s => s.CarNumber == normalized))
        {
            throw new InvalidOperationException($"Автомобиль {normalized} уже находится на парковке.");
        }

        spot.Status = SpotStatus.Occupied;
        spot.CarNumber = normalized;
        spot.OccupiedSince = DateTime.Now;
        _spots.Update(spot);
    }

    public void RegisterExit(int spotId)
    {
        var spot = _spots.GetById(spotId) ?? throw new InvalidOperationException("Парковочное место не найдено.");

        if (spot.Status != SpotStatus.Occupied)
        {
            throw new InvalidOperationException($"Место {spot.Number} не занято.");
        }

        spot.Status = SpotStatus.Free;
        spot.CarNumber = null;
        spot.OccupiedSince = null;
        _spots.Update(spot);
    }
}
