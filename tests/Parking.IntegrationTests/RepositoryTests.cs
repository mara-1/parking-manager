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
    [Trait("Task", "7")]
    public void SessionRepository_AddAndUpdate_PersistsSession()
    {
        var repository = new SessionRepository(_database.Context);
        var session = new ParkingSession
        {
            CarNumber = "А123ВС73",
            Category = SpotCategory.Disabled,
            EntryTime = new DateTime(2026, 10, 24, 9, 15, 0),
        };
        repository.Add(session);
        session.ExitTime = new DateTime(2026, 10, 24, 11, 0, 0);
        repository.Update(session);
        _database.Context.ChangeTracker.Clear();

        var reloaded = repository.GetById(session.Id);

        Assert.NotNull(reloaded);
        Assert.Equal("А123ВС73", reloaded!.CarNumber);
        Assert.Equal(SpotCategory.Disabled, reloaded.Category);
        Assert.Equal(new DateTime(2026, 10, 24, 11, 0, 0), reloaded.ExitTime);
    }

    [Fact]
    [Trait("Task", "6")]
    public void SessionRepository_CountAndFindActive_IgnoreFinishedSessions()
    {
        var repository = new SessionRepository(_database.Context);
        var entry = new DateTime(2026, 10, 24, 9, 0, 0);
        repository.Add(new ParkingSession { CarNumber = "А111АА73", Category = SpotCategory.Regular, EntryTime = entry });
        repository.Add(new ParkingSession { CarNumber = "В222ВВ73", Category = SpotCategory.Regular, EntryTime = entry, ExitTime = entry.AddHours(1) });
        repository.Add(new ParkingSession { CarNumber = "Е333ЕЕ73", Category = SpotCategory.Disabled, EntryTime = entry });

        Assert.Equal(1, repository.CountActive(SpotCategory.Regular));
        Assert.Equal(1, repository.CountActive(SpotCategory.Disabled));
        Assert.NotNull(repository.FindActiveByCarNumber("А111АА73"));
        Assert.Null(repository.FindActiveByCarNumber("В222ВВ73"));
        Assert.Equal("Е333ЕЕ73", Assert.Single(repository.GetActive("333")).CarNumber);
    }

    [Fact]
    [Trait("Task", "7")]
    public void SessionRepository_GetByEntryPeriod_FiltersByDateAndNumber()
    {
        var repository = new SessionRepository(_database.Context);
        repository.Add(new ParkingSession { CarNumber = "А111АА73", EntryTime = new DateTime(2026, 10, 23, 23, 50, 0) });
        repository.Add(new ParkingSession { CarNumber = "В222ВВ73", EntryTime = new DateTime(2026, 10, 24, 8, 0, 0) });
        repository.Add(new ParkingSession { CarNumber = "Е333ЕЕ73", EntryTime = new DateTime(2026, 10, 24, 18, 0, 0) });

        var day = repository.GetByEntryPeriod(new DateTime(2026, 10, 24), new DateTime(2026, 10, 25), string.Empty);
        var byNumber = repository.GetByEntryPeriod(new DateTime(2026, 10, 24), new DateTime(2026, 10, 25), "222");

        Assert.Collection(
            day,
            first => Assert.Equal("Е333ЕЕ73", first.CarNumber),
            second => Assert.Equal("В222ВВ73", second.CarNumber));
        Assert.Equal("В222ВВ73", Assert.Single(byNumber).CarNumber);
    }

    [Fact]
    [Trait("Task", "6")]
    public void CategoryRepository_Update_PersistsCapacity()
    {
        var repository = new CategoryRepository(_database.Context);
        var regular = repository.Get(SpotCategory.Regular)!;

        regular.Capacity = 400;
        repository.Update(regular);
        _database.Context.ChangeTracker.Clear();

        Assert.Equal(400, repository.Get(SpotCategory.Regular)!.Capacity);
        Assert.Collection(
            repository.GetAll(),
            first => Assert.Equal(SpotCategory.Regular, first.Category),
            second => Assert.Equal(SpotCategory.Disabled, second.Category));
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
