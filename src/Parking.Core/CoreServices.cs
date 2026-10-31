using Microsoft.EntityFrameworkCore;
using Parking.Core.DataAccess;
using Parking.Core.Services;

namespace Parking.Core;

// Точка сборки ядра: слой представления получает готовые службы
// и не зависит напрямую от EF Core и репозиториев.
public sealed class CoreServices : IDisposable
{
    private readonly ParkingDbContext _dbContext;

    private CoreServices(ParkingDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        Auth = new AuthService(new OperatorRepository(dbContext));
        Parking = new ParkingService(new CategoryRepository(dbContext), new SessionRepository(dbContext), timeProvider);
    }

    public IAuthService Auth { get; }

    public IParkingService Parking { get; }

    public static CoreServices Create(string databasePath)
    {
        return Create(databasePath, TimeProvider.System);
    }

    // Источник времени передаётся снаружи, чтобы тесты могли задавать время въезда и выезда.
    public static CoreServices Create(string databasePath, TimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrEmpty(databasePath);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var options = new DbContextOptionsBuilder<ParkingDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;
        var dbContext = new ParkingDbContext(options);
        DatabaseInitializer.Initialize(dbContext);
        return new CoreServices(dbContext, timeProvider);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
