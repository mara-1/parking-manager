using System.Diagnostics.CodeAnalysis;
using System.Windows;
using ParkingManager.ViewModels;

namespace ParkingManager.Views;

[ExcludeFromCodeCoverage(Justification = "Окно: проверяется UI-тестами (FlaUI), покрытие которых не собирается.")]
public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    // PasswordBox не поддерживает привязку пароля, поэтому значение
    // передаётся в модель представления из обработчика события.
    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = PasswordInput.Password;
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        DialogResult = true;
    }
}
