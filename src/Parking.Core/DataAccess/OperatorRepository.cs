using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public class OperatorRepository : IOperatorRepository
{
    private readonly ParkingDbContext _dbContext;

    public OperatorRepository(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public OperatorAccount? FindByLogin(string login)
    {
        return _dbContext.Operators.FirstOrDefault(o => o.Login == login);
    }
}
