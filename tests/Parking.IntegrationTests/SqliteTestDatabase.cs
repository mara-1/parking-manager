using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Parking.Core.DataAccess;

namespace Parking.IntegrationTests;

public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ParkingDbContext>()
            .UseSqlite(_connection)
            .Options;
        Context = new ParkingDbContext(options);
        Context.Database.EnsureCreated();
    }

    public ParkingDbContext Context { get; }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
