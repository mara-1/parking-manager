namespace Parking.Core.Models;

// Стоянка автомобиля: от въезда до выезда. Пока выезда нет, стоянка активна
// и занимает одно место своей категории. Завершённые стоянки образуют историю.
public class ParkingSession
{
    public int Id { get; set; }

    public string CarNumber { get; set; } = string.Empty;

    public SpotCategory Category { get; set; }

    public DateTime EntryTime { get; set; }

    public DateTime? ExitTime { get; set; }

    // Сумма к оплате; рассчитывается при выезде (задача №8 «Тарификация»).
    public decimal? Amount { get; set; }

    public bool IsActive => ExitTime is null;
}
