using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Task", "3")]
[Trait("Task", "6")]
public sealed class DatabaseInitializerTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    [Fact]
    public void Initialize_EmptyDatabase_SeedsDemoOperatorAndCapacities()
    {
        DatabaseInitializer.Initialize(_database.Context);

        Assert.Single(_database.Context.Operators);
        var categories = _database.Context.Categories.ToList();
        Assert.Equal(DatabaseInitializer.RegularCapacity, categories.Single(c => c.Category == SpotCategory.Regular).Capacity);
        Assert.Equal(DatabaseInitializer.DisabledCapacity, categories.Single(c => c.Category == SpotCategory.Disabled).Capacity);
    }

    [Fact]
    public void Initialize_CalledTwice_DoesNotDuplicateData()
    {
        DatabaseInitializer.Initialize(_database.Context);
        DatabaseInitializer.Initialize(_database.Context);

        Assert.Single(_database.Context.Operators);
        Assert.Equal(2, _database.Context.Categories.Count());
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
