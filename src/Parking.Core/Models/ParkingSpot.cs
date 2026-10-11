namespace Parking.Core.Models;

public class ParkingSpot
{
    public int Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public SpotStatus Status { get; set; }

    public string? CarNumber { get; set; }

    public DateTime? OccupiedSince { get; set; }
}
