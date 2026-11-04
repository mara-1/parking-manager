using Microsoft.EntityFrameworkCore;
using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public static class DatabaseInitializer
{
    public const int RegularCapacity = 450;
    public const int DisabledCapacity = 50;
    public const decimal RegularPricePerHour = 60m;
    public const decimal DisabledPricePerHour = 0m;

    // Версия схемы базы. Увеличивается при каждом изменении модели данных
    // (2 — учёт по категориям, этап 3; 3 — тарифы; 4 — брони, этап 4).
    public const int SchemaVersion = 4;

    public static void Initialize(ParkingDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        // На этапах разработки (до выпуска 1.0) схема менялась на каждом этапе,
        // а база содержала только тестовые данные, поэтому база старой версии
        // пересоздаётся. Дальнейшие изменения схемы потребуют миграций EF Core.
        if (HasTables(dbContext) && GetSchemaVersion(dbContext) != SchemaVersion)
        {
            dbContext.Database.EnsureDeleted();
        }

        if (dbContext.Database.EnsureCreated())
        {
            // Число должно совпадать с SchemaVersion.
            dbContext.Database.ExecuteSqlRaw("PRAGMA user_version = 4");
        }

        Seed(dbContext);
    }

    public static int GetSchemaVersion(ParkingDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return dbContext.Database
            .SqlQuery<int>($"SELECT user_version AS \"Value\" FROM pragma_user_version")
            .AsEnumerable()
            .Single();
    }

    private static void Seed(ParkingDbContext dbContext)
    {
        // Учётная запись оператора не создаётся заранее: оператор
        // регистрируется сам при первом запуске программы.
        if (!dbContext.Categories.Any())
        {
            dbContext.Categories.Add(new CategoryCapacity { Category = SpotCategory.Regular, Capacity = RegularCapacity });
            dbContext.Categories.Add(new CategoryCapacity { Category = SpotCategory.Disabled, Capacity = DisabledCapacity });
        }

        if (!dbContext.Tariffs.Any())
        {
            dbContext.Tariffs.Add(new Tariff { Category = SpotCategory.Regular, PricePerHour = RegularPricePerHour });
            dbContext.Tariffs.Add(new Tariff { Category = SpotCategory.Disabled, PricePerHour = DisabledPricePerHour });
        }

        dbContext.SaveChanges();
    }

    private static bool HasTables(ParkingDbContext dbContext)
    {
        return dbContext.Database
            .SqlQuery<int>($"SELECT COUNT(*) AS \"Value\" FROM sqlite_master WHERE type = 'table'")
            .AsEnumerable()
            .Single() > 0;
    }
}
