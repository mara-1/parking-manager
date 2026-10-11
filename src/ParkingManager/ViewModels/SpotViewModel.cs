using Parking.Core.Models;

namespace ParkingManager.ViewModels;

public sealed class SpotViewModel
{
    public SpotViewModel(ParkingSpot spot)
    {
        ArgumentNullException.ThrowIfNull(spot);

        Id = spot.Id;
        Number = spot.Number;
        Status = spot.Status;
        CarNumber = spot.CarNumber;
        OccupiedSince = spot.OccupiedSince;
    }

    public int Id { get; }

    public string Number { get; }

    public SpotStatus Status { get; }

    public string? CarNumber { get; }

    public DateTime? OccupiedSince { get; }

    public bool IsFree => Status == SpotStatus.Free;

    public bool IsOccupied => Status == SpotStatus.Occupied;

    public string StatusText => Status switch
    {
        SpotStatus.Free => "Свободно",
        SpotStatus.Occupied => "Занято",
        SpotStatus.Booked => "Забронировано",
        _ => Status.ToString(),
    };

    public string Details => IsOccupied ? $"{CarNumber}\nс {OccupiedSince:HH:mm}" : StatusText;
}
