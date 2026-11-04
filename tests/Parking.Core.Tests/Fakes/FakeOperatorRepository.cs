using Parking.Core.DataAccess;
using Parking.Core.Models;
using Parking.Core.Services;

namespace Parking.Core.Tests.Fakes;

public class FakeOperatorRepository : IOperatorRepository
{
    private readonly List<OperatorAccount> _accounts = new List<OperatorAccount>();

    public IReadOnlyList<OperatorAccount> Accounts => _accounts;

    public void Add(string login, string password)
    {
        var (hash, salt) = PasswordHasher.Hash(password);
        Add(new OperatorAccount
        {
            Login = login,
            FullName = login,
            PasswordHash = hash,
            PasswordSalt = salt,
        });
    }

    public void Add(OperatorAccount account)
    {
        account.Id = _accounts.Count + 1;
        _accounts.Add(account);
    }

    public bool Any()
    {
        return _accounts.Count > 0;
    }

    public OperatorAccount? FindByLogin(string login)
    {
        return _accounts.Find(a => a.Login == login);
    }
}
