using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public interface ISessionRepository
{
    void Add(ParkingSession session);

    void Update(ParkingSession session);

    ParkingSession? GetById(int id);

    ParkingSession? FindActiveByCarNumber(string carNumber);

    int CountActive(SpotCategory category);

    // Автомобили на парковке; carNumberPart — часть номера для поиска или пустая строка.
    IReadOnlyList<ParkingSession> GetActive(string carNumberPart);

    // Стоянки с въездом в периоде [from, to); carNumberPart — часть номера или пустая строка.
    IReadOnlyList<ParkingSession> GetByEntryPeriod(DateTime from, DateTime to, string carNumberPart);
}
