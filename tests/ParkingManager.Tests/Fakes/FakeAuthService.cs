using Parking.Core.Models;
using Parking.Core.Services;

namespace ParkingManager.Tests.Fakes;

// Упрощённая служба авторизации: один оператор с известным паролем.
public sealed class FakeAuthService : IAuthService
{
    public const string ValidLogin = "operator";
    public const string ValidPassword = "pass123";
    public const string OperatorFullName = "Иванова Мария";

    public OperatorAccount? CurrentOperator { get; private set; }

    public bool HasOperator { get; set; } = true;

    // Сообщение, с которым RegisterOperator отклоняет регистрацию; null — регистрация успешна.
    public string? RegistrationError { get; set; }

    public bool Login(string login, string password)
    {
        if (login == ValidLogin && password == ValidPassword)
        {
            CurrentOperator = new OperatorAccount { Id = 1, Login = login, FullName = OperatorFullName };
            return true;
        }

        CurrentOperator = null;
        return false;
    }

    public void RegisterOperator(string fullName, string login, string password)
    {
        if (RegistrationError is not null)
        {
            throw new ParkingOperationException(RegistrationError);
        }

        HasOperator = true;
        CurrentOperator = new OperatorAccount { Id = 1, Login = login, FullName = fullName };
    }

    public void Logout()
    {
        CurrentOperator = null;
    }
}
