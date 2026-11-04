using Parking.Core.Models;

namespace Parking.Core.Services;

public interface IAuthService
{
    OperatorAccount? CurrentOperator { get; }

    // Зарегистрирован ли оператор. Если нет — при запуске
    // показывается окно регистрации вместо окна входа.
    bool HasOperator { get; }

    bool Login(string login, string password);

    // Регистрирует единственного оператора и выполняет вход под ним.
    void RegisterOperator(string fullName, string login, string password);

    void Logout();
}
