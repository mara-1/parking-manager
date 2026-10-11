using Parking.Core.DataAccess;
using Parking.Core.Models;
using Parking.Core.Services;

namespace Parking.Core.Tests.Fakes;

public class FakeOperatorRepository : IOperatorRepository
{
    private readonly List<OperatorAccount> _accounts = new List<OperatorAccount>();

    public void Add(string login, string password)
    {
        var (hash, salt) = PasswordHasher.Hash(password);
        _accounts.Add(new OperatorAccount
        {
            Id = _accounts.Count + 1,
            Login = login,
            FullName = login,
            PasswordHash = hash,
            PasswordSalt = salt,
        });
    }

    public OperatorAccount? FindByLogin(string login)
    {
        return _accounts.Find(a => a.Login == login);
    }
}
