using Microsoft.EntityFrameworkCore;
using Parking.Core.Models;
using Parking.Core.Services;

namespace Parking.Core.DataAccess;

public static class DatabaseInitializer
{
    public const string DemoLogin = "admin";
    public const string DemoPassword = "admin123";
    public const int RegularCapacity = 450;
    public const int DisabledCapacity = 50;
    public const decimal RegularPricePerHour = 60m;
    public const decimal DisabledPricePerHour = 0m;

    // Версия схемы базы. Увеличивается при каждом изменении модели данных
    // (2 — учёт по категориям, этап 3; 3 — тарифы, этап 4).
    public const int SchemaVersion = 3;

    public static void Initialize(ParkingDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        // До выпуска 1.0 база содержит только демонстрационные данные, поэтому
        // база старой версии схемы пересоздаётся, а не переносится миграциями.
        if (HasTables(dbContext) && GetSchemaVersion(dbContext) != SchemaVersion)
        {
            dbContext.Database.EnsureDeleted();
        }

        if (dbContext.Database.EnsureCreated())
        {
            // Число должно совпадать с SchemaVersion.
            dbContext.Database.ExecuteSqlRaw("PRAGMA user_version = 3");
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
        if (!dbContext.Operators.Any())
        {
            var (hash, salt) = PasswordHasher.Hash(DemoPassword);
            dbContext.Operators.Add(new OperatorAccount
            {
                Login = DemoLogin,
                FullName = "Демо-оператор",
                PasswordHash = hash,
                PasswordSalt = salt,
            });
        }

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
