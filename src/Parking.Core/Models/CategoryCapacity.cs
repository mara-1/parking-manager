namespace Parking.Core.Models;

// Вместимость парковки по категории мест (например, 450 обычных и 50 для инвалидов).
public class CategoryCapacity
{
    public SpotCategory Category { get; set; }

    public int Capacity { get; set; }
}
