using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Tests.Fakes;

public class FakeBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = new List<Booking>();

    public IReadOnlyList<Booking> All => _bookings;

    public void Add(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        booking.Id = _bookings.Count + 1;
        _bookings.Add(booking);
    }

    public void Update(Booking booking)
    {
        // Объекты хранятся по ссылке, изменения уже применены.
    }

    public Booking? GetById(int id)
    {
        return _bookings.Find(b => b.Id == id);
    }

    public Booking? FindWaitingByCarNumber(string carNumber)
    {
        return _bookings.Find(b => b.Status == BookingStatus.Waiting && b.CarNumber == carNumber);
    }

    public int CountWaiting(SpotCategory category)
    {
        return _bookings.Count(b => b.Status == BookingStatus.Waiting && b.Category == category);
    }

    public IReadOnlyList<Booking> GetWaiting()
    {
        return _bookings.Where(b => b.Status == BookingStatus.Waiting).ToList();
    }

    public IReadOnlyList<Booking> GetByArrivalPeriod(DateTime from, DateTime until)
    {
        return _bookings.Where(b => b.ArrivalTime >= from && b.ArrivalTime < until).OrderBy(b => b.ArrivalTime).ToList();
    }
}
