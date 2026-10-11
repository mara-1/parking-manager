using Parking.Core.Models;

namespace Parking.Core.Services;

public interface IAuthService
{
    OperatorAccount? CurrentOperator { get; }

    bool Login(string login, string password);

    void Logout();
}
