using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public class OperatorRepository : IOperatorRepository
{
    private readonly ParkingDbContext _dbContext;

    public OperatorRepository(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public bool Any()
    {
        return _dbContext.Operators.Any();
    }

    public OperatorAccount? FindByLogin(string login)
    {
        return _dbContext.Operators.FirstOrDefault(o => o.Login == login);
    }

    public void Add(OperatorAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        _dbContext.Operators.Add(account);
        _dbContext.SaveChanges();
    }
}
