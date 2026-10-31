using Microsoft.EntityFrameworkCore;
using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public class SessionRepository : ISessionRepository
{
    private readonly ParkingDbContext _dbContext;

    public SessionRepository(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(ParkingSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _dbContext.Sessions.Add(session);
        _dbContext.SaveChanges();
    }

    public void Update(ParkingSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _dbContext.Sessions.Update(session);
        _dbContext.SaveChanges();
    }

    public ParkingSession? GetById(int id)
    {
        return _dbContext.Sessions.Find(id);
    }

    public ParkingSession? FindActiveByCarNumber(string carNumber)
    {
        return _dbContext.Sessions.FirstOrDefault(s => s.ExitTime == null && s.CarNumber == carNumber);
    }

    public int CountActive(SpotCategory category)
    {
        return _dbContext.Sessions.Count(s => s.ExitTime == null && s.Category == category);
    }

    public IReadOnlyList<ParkingSession> GetActive(string carNumberPart)
    {
        var query = _dbContext.Sessions.Where(s => s.ExitTime == null);
        return Filter(query, carNumberPart).OrderByDescending(s => s.EntryTime).ToList();
    }

    public IReadOnlyList<ParkingSession> GetByEntryPeriod(DateTime from, DateTime to, string carNumberPart)
    {
        var query = _dbContext.Sessions.Where(s => s.EntryTime >= from && s.EntryTime < to);
        return Filter(query, carNumberPart).OrderByDescending(s => s.EntryTime).ToList();
    }

    // Номер нормализован (только буквы и цифры), поэтому спецсимволов LIKE в нём нет.
    private static IQueryable<ParkingSession> Filter(IQueryable<ParkingSession> query, string carNumberPart)
    {
        return string.IsNullOrEmpty(carNumberPart)
            ? query
            : query.Where(s => EF.Functions.Like(s.CarNumber, "%" + carNumberPart + "%"));
    }
}
