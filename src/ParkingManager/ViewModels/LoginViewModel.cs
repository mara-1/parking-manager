using System.Windows.Input;
using Parking.Core.Services;

namespace ParkingManager.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        LoginCommand = new RelayCommand(ExecuteLogin, CanLogin);
    }

    public event EventHandler? LoginSucceeded;

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public ICommand LoginCommand { get; }

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrEmpty(Password);
    }

    private void ExecuteLogin()
    {
        if (_authService.Login(Login, Password))
        {
            ErrorMessage = string.Empty;
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            ErrorMessage = "Неверный логин или пароль.";
        }
    }
}
