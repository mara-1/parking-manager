using System.Windows.Input;
using Parking.Core.Services;

namespace ParkingManager.ViewModels;

// Регистрация оператора при первом запуске программы.
public sealed class RegistrationViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private string _fullName = string.Empty;
    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;

    public RegistrationViewModel(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        RegisterCommand = new RelayCommand(ExecuteRegister, CanRegister);
    }

    public event EventHandler? RegistrationSucceeded;

    public string FullName
    {
        get => _fullName;
        set => SetField(ref _fullName, value);
    }

    public string Login
    {
        get => _login;
        set => SetField(ref _login, value);
    }

    public string Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetField(ref _confirmPassword, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public ICommand RegisterCommand { get; }

    private void SetField(ref string field, string value)
    {
        if (SetProperty(ref field, value))
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private bool CanRegister()
    {
        return !string.IsNullOrWhiteSpace(FullName)
            && !string.IsNullOrWhiteSpace(Login)
            && !string.IsNullOrEmpty(Password)
            && !string.IsNullOrEmpty(ConfirmPassword);
    }

    private void ExecuteRegister()
    {
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают.";
            return;
        }

        try
        {
            _authService.RegisterOperator(FullName, Login, Password);
        }
        catch (ParkingOperationException ex)
        {
            ErrorMessage = ex.Message;
            return;
        }

        ErrorMessage = string.Empty;
        RegistrationSucceeded?.Invoke(this, EventArgs.Empty);
    }
}
