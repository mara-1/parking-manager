using Parking.Core.DataAccess;

namespace Parking.IntegrationTests;

public sealed class DatabaseInitializerTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    [Fact]
    public void Initialize_EmptyDatabase_SeedsDemoOperatorAndSpots()
    {
        DatabaseInitializer.Initialize(_database.Context);

        Assert.Single(_database.Context.Operators);
        Assert.Equal(DatabaseInitializer.DemoSpotCount, _database.Context.ParkingSpots.Count());
    }

    [Fact]
    public void Initialize_CalledTwice_DoesNotDuplicateData()
    {
        DatabaseInitializer.Initialize(_database.Context);
        DatabaseInitializer.Initialize(_database.Context);

        Assert.Single(_database.Context.Operators);
        Assert.Equal(DatabaseInitializer.DemoSpotCount, _database.Context.ParkingSpots.Count());
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
