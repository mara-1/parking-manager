using System.Diagnostics.CodeAnalysis;
using System.Windows;
using ParkingManager.ViewModels;

namespace ParkingManager.Views;

[ExcludeFromCodeCoverage(Justification = "Окно: проверяется UI-тестами (FlaUI), покрытие которых не собирается.")]
public partial class RegistrationWindow : Window
{
    private readonly RegistrationViewModel _viewModel;

    public RegistrationWindow(RegistrationViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.RegistrationSucceeded += OnRegistrationSucceeded;
    }

    // PasswordBox не поддерживает привязку пароля, поэтому значения
    // передаются в модель представления из обработчиков событий.
    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = PasswordInput.Password;
    }

    private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.ConfirmPassword = ConfirmPasswordInput.Password;
    }

    private void OnRegistrationSucceeded(object? sender, EventArgs e)
    {
        DialogResult = true;
    }
}
