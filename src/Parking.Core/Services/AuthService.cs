using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Services;

public class AuthService : IAuthService
{
    public const int MinPasswordLength = 6;

    private readonly IOperatorRepository _operators;

    public AuthService(IOperatorRepository operators)
    {
        _operators = operators ?? throw new ArgumentNullException(nameof(operators));
    }

    public OperatorAccount? CurrentOperator { get; private set; }

    public bool HasOperator => _operators.Any();

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

    // Программа рассчитана на одного оператора, поэтому учётная запись
    // создаётся один раз — при первом запуске на рабочей станции.
    public void RegisterOperator(string fullName, string login, string password)
    {
        if (HasOperator)
        {
            throw new ParkingOperationException("Оператор уже зарегистрирован. Войдите под своим логином.");
        }

        ValidateRegistration(fullName, login, password);

        var (hash, salt) = PasswordHasher.Hash(password);
        var account = new OperatorAccount
        {
            FullName = fullName.Trim(),
            Login = login.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
        };
        _operators.Add(account);
        CurrentOperator = account;
    }

    public void Logout()
    {
        CurrentOperator = null;
    }

    private static void ValidateRegistration(string fullName, string login, string password)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ParkingOperationException("Укажите ФИО оператора.");
        }

        if (string.IsNullOrWhiteSpace(login))
        {
            throw new ParkingOperationException("Укажите логин.");
        }

        if (password is null || password.Length < MinPasswordLength)
        {
            throw new ParkingOperationException($"Пароль должен содержать не менее {MinPasswordLength} символов.");
        }
    }
}
