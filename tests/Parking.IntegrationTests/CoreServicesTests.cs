using Microsoft.Data.Sqlite;
using Parking.Core;
using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Task", "2")]
public sealed class CoreServicesTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"parking-test-{Guid.NewGuid():N}.db");

    [Fact]
    [Trait("Task", "5")]
    [Trait("Task", "7")]
    public void Create_NewDatabase_AllowsDemoLoginEntryAndExitAcrossRestarts()
    {
        int sessionId;
        using (var core = CoreServices.Create(_databasePath))
        {
            Assert.True(core.Auth.Login(DatabaseInitializer.DemoLogin, DatabaseInitializer.DemoPassword));
            sessionId = core.Parking.RegisterEntry("А123ВС73", SpotCategory.Regular).Id;
        }

        using (var reopened = CoreServices.Create(_databasePath))
        {
            Assert.Equal("А123ВС73", Assert.Single(reopened.Parking.GetActiveSessions(string.Empty)).CarNumber);
            Assert.Equal(DatabaseInitializer.RegularCapacity - 1, reopened.Parking.GetOccupancy().Single(o => o.Category == SpotCategory.Regular).Free);
            Assert.Equal(60m, reopened.Parking.RegisterExit(sessionId).Amount);
        }

        using (var third = CoreServices.Create(_databasePath))
        {
            Assert.Empty(third.Parking.GetActiveSessions(string.Empty));
        }
    }

    [Fact]
    [Trait("Task", "3")]
    public void Create_PrototypeDatabase_RecreatesWithNewSchema()
    {
        using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE ParkingSpots (Id INTEGER PRIMARY KEY, Number TEXT NOT NULL);";
            command.ExecuteNonQuery();
        }

        SqliteConnection.ClearAllPools();

        using var core = CoreServices.Create(_databasePath);

        Assert.Equal(2, core.Parking.GetOccupancy().Count);
        Assert.Empty(core.Parking.GetActiveSessions(string.Empty));
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
