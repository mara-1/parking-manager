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

    public static void Initialize(ParkingDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        // База прототипа (этап 2) хранила конкретные места в таблице ParkingSpots.
        // Её схема несовместима с учётом по категориям, а данные демонстрационные,
        // поэтому такая база пересоздаётся.
        if (TableExists(dbContext, "ParkingSpots"))
        {
            dbContext.Database.EnsureDeleted();
        }

        dbContext.Database.EnsureCreated();

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

        dbContext.SaveChanges();
    }

    private static bool TableExists(ParkingDbContext dbContext, string tableName)
    {
        return dbContext.Database
            .SqlQuery<int>($"SELECT COUNT(*) AS \"Value\" FROM sqlite_master WHERE type = 'table' AND name = {tableName}")
            .AsEnumerable()
            .Single() > 0;
    }
}
