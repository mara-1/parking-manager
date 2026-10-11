using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Task", "3")]
public sealed class RepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public RepositoryTests()
    {
        DatabaseInitializer.Initialize(_database.Context);
    }

    [Fact]
    [Trait("Task", "5")]
    public void OperatorRepository_FindByLogin_ReturnsSeededOperator()
    {
        var repository = new OperatorRepository(_database.Context);

        var account = repository.FindByLogin(DatabaseInitializer.DemoLogin);

        Assert.NotNull(account);
        Assert.Null(repository.FindByLogin("unknown"));
    }

    [Fact]
    [Trait("Task", "6")]
    public void ParkingSpotRepository_Update_PersistsChanges()
    {
        var repository = new ParkingSpotRepository(_database.Context);
        var spot = repository.GetAll()[0];

        spot.Status = SpotStatus.Occupied;
        spot.CarNumber = "А123ВС73";
        repository.Update(spot);
        _database.Context.ChangeTracker.Clear();

        var reloaded = repository.GetById(spot.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(SpotStatus.Occupied, reloaded!.Status);
        Assert.Equal("А123ВС73", reloaded.CarNumber);
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
