using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Tests.Fakes;

// Стоянки в памяти с теми же правилами выборки, что у SessionRepository.
public class FakeSessionRepository : ISessionRepository
{
    private readonly List<ParkingSession> _sessions = new List<ParkingSession>();

    public int SaveCount { get; private set; }

    public IReadOnlyList<ParkingSession> All => _sessions;

    public void Add(ParkingSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        session.Id = _sessions.Count + 1;
        _sessions.Add(session);
        SaveCount++;
    }

    public void Update(ParkingSession session)
    {
        SaveCount++;
    }

    public ParkingSession? GetById(int id)
    {
        return _sessions.Find(s => s.Id == id);
    }

    public ParkingSession? FindActiveByCarNumber(string carNumber)
    {
        return _sessions.Find(s => s.IsActive && s.CarNumber == carNumber);
    }

    public int CountActive(SpotCategory category)
    {
        return _sessions.Count(s => s.IsActive && s.Category == category);
    }

    public IReadOnlyList<ParkingSession> GetActive(string carNumberPart)
    {
        return _sessions
            .Where(s => s.IsActive && s.CarNumber.Contains(carNumberPart, StringComparison.Ordinal))
            .OrderByDescending(s => s.EntryTime)
            .ToList();
    }

    public IReadOnlyList<ParkingSession> GetByEntryPeriod(DateTime from, DateTime until, string carNumberPart)
    {
        return _sessions
            .Where(s => s.EntryTime >= from && s.EntryTime < until)
            .Where(s => s.CarNumber.Contains(carNumberPart, StringComparison.Ordinal))
            .OrderByDescending(s => s.EntryTime)
            .ToList();
    }
}
