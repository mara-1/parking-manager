using Microsoft.Data.Sqlite;
using Parking.Core;
using Parking.Core.DataAccess;

namespace Parking.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Task", "2")]
[Trait("Task", "5")]
[Trait("Task", "7")]
public sealed class CoreServicesTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"parking-test-{Guid.NewGuid():N}.db");

    [Fact]
    public void Create_NewDatabase_AllowsDemoLoginAndEntryRegistration()
    {
        using (var core = CoreServices.Create(_databasePath))
        {
            Assert.True(core.Auth.Login(DatabaseInitializer.DemoLogin, DatabaseInitializer.DemoPassword));

            var spot = core.Parking.GetSpots()[0];
            core.Parking.RegisterEntry(spot.Id, "А123ВС73");
        }

        using (var reopened = CoreServices.Create(_databasePath))
        {
            Assert.Contains(reopened.Parking.GetSpots(), s => s.CarNumber == "А123ВС73");
        }
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
