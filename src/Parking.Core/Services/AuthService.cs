using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Services;

public class AuthService : IAuthService
{
    private readonly IOperatorRepository _operators;

    public AuthService(IOperatorRepository operators)
    {
        _operators = operators ?? throw new ArgumentNullException(nameof(operators));
    }

    public OperatorAccount? CurrentOperator { get; private set; }

    public bool Login(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        var account = _operators.FindByLogin(login.Trim());
        if (account is null || !PasswordHasher.Verify(password, account.PasswordHash, account.PasswordSalt))
        {
            return false;
        }

        CurrentOperator = account;
        return true;
    }

    public void Logout()
    {
        CurrentOperator = null;
    }
}
