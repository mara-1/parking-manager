using Parking.Core.Models;

namespace Parking.Core.Services;

public interface IParkingService
{
    // Вместимость и занятость по каждой категории мест.
    IReadOnlyList<OccupancyInfo> GetOccupancy();

    // Автомобили на парковке; carNumberPart — часть номера для поиска или пустая строка.
    IReadOnlyList<ParkingSession> GetActiveSessions(string carNumberPart);

    ParkingSession RegisterEntry(string carNumber, SpotCategory category);

    ParkingSession RegisterExit(int sessionId);

    // Сумма к оплате: для завершённой стоянки — зафиксированная при выезде,
    // для активной — по действующему тарифу на текущий момент.
    decimal CalculateAmount(ParkingSession session);

    // История: стоянки с въездом в периоде [from, to), включая ещё не завершённые.
    IReadOnlyList<ParkingSession> GetHistory(DateTime from, DateTime until, string carNumberPart);
}
