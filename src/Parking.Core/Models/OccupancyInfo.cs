namespace Parking.Core.Models;

// Занятость одной категории мест в текущий момент.
public sealed class OccupancyInfo
{
    public SpotCategory Category { get; init; }

    public int Capacity { get; init; }

    public int Occupied { get; init; }

    // Места, удерживаемые бронями (задача №9 «Бронирование»).
    public int Booked { get; init; }

    public int Free => Math.Max(0, Capacity - Occupied - Booked);
}
