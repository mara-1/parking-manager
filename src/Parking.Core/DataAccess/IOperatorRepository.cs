using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public interface IOperatorRepository
{
    OperatorAccount? FindByLogin(string login);
}
