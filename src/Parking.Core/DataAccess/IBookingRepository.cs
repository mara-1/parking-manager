using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public interface IBookingRepository
{
    void Add(Booking booking);

    void Update(Booking booking);

    Booking? GetById(int id);

    Booking? FindWaitingByCarNumber(string carNumber);

    int CountWaiting(SpotCategory category);

    IReadOnlyList<Booking> GetWaiting();

    // Брони с временем прибытия в периоде [from, to), по возрастанию времени.
    IReadOnlyList<Booking> GetByArrivalPeriod(DateTime from, DateTime until);
}
