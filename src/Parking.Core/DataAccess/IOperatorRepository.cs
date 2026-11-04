using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public interface IOperatorRepository
{
    bool Any();

    OperatorAccount? FindByLogin(string login);

    void Add(OperatorAccount account);
}
