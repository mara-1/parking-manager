using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public class BookingRepository : IBookingRepository
{
    private readonly ParkingDbContext _dbContext;

    public BookingRepository(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        _dbContext.Bookings.Add(booking);
        _dbContext.SaveChanges();
    }

    public void Update(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        _dbContext.Bookings.Update(booking);
        _dbContext.SaveChanges();
    }

    public Booking? GetById(int id)
    {
        return _dbContext.Bookings.Find(id);
    }

    public Booking? FindWaitingByCarNumber(string carNumber)
    {
        return _dbContext.Bookings.FirstOrDefault(b => b.Status == BookingStatus.Waiting && b.CarNumber == carNumber);
    }

    public int CountWaiting(SpotCategory category)
    {
        return _dbContext.Bookings.Count(b => b.Status == BookingStatus.Waiting && b.Category == category);
    }

    public IReadOnlyList<Booking> GetWaiting()
    {
        return _dbContext.Bookings.Where(b => b.Status == BookingStatus.Waiting).ToList();
    }

    public IReadOnlyList<Booking> GetByArrivalPeriod(DateTime from, DateTime until)
    {
        return _dbContext.Bookings
            .Where(b => b.ArrivalTime >= from && b.ArrivalTime < until)
            .OrderBy(b => b.ArrivalTime)
            .ToList();
    }
}
