using Parking.Core.Models;
using Parking.Core.Services;

namespace Parking.Core.DataAccess;

public static class DatabaseInitializer
{
    public const string DemoLogin = "admin";
    public const string DemoPassword = "admin123";
    public const int DemoSpotCount = 20;

    public static void Initialize(ParkingDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

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

        if (!dbContext.ParkingSpots.Any())
        {
            for (var i = 1; i <= DemoSpotCount; i++)
            {
                dbContext.ParkingSpots.Add(new ParkingSpot { Number = $"A-{i:D2}", Status = SpotStatus.Free });
            }
        }

        dbContext.SaveChanges();
    }
}
