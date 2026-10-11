using ParkingManager.Tests.Fakes;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
[Trait("Task", "5")]
public class LoginViewModelTests
{
    private readonly FakeAuthService _authService = new FakeAuthService();
    private readonly LoginViewModel _viewModel;

    public LoginViewModelTests()
    {
        _viewModel = new LoginViewModel(_authService);
    }

    [Fact]
    public void LoginCommand_EmptyFields_CannotExecute()
    {
        Assert.False(_viewModel.LoginCommand.CanExecute(null));
    }

    [Theory]
    [InlineData("operator", "")]
    [InlineData("   ", "pass123")]
    public void LoginCommand_OneFieldEmpty_CannotExecute(string login, string password)
    {
        _viewModel.Login = login;
        _viewModel.Password = password;

        Assert.False(_viewModel.LoginCommand.CanExecute(null));
    }

    [Fact]
    public void LoginCommand_LoginAndPasswordFilled_CanExecute()
    {
        _viewModel.Login = FakeAuthService.ValidLogin;
        _viewModel.Password = FakeAuthService.ValidPassword;

        Assert.True(_viewModel.LoginCommand.CanExecute(null));
    }

    [Fact]
    public void Login_WrongPassword_ShowsErrorAndDoesNotRaiseEvent()
    {
        var raised = false;
        _viewModel.LoginSucceeded += (sender, args) => raised = true;
        _viewModel.Login = FakeAuthService.ValidLogin;
        _viewModel.Password = "wrong";

        _viewModel.LoginCommand.Execute(null);

        Assert.False(raised);
        Assert.Equal("Неверный логин или пароль.", _viewModel.ErrorMessage);
        Assert.Null(_authService.CurrentOperator);
    }

    [Fact]
    public void Login_ValidCredentialsAfterError_RaisesEventAndClearsError()
    {
        var raised = false;
        _viewModel.LoginSucceeded += (sender, args) => raised = true;
        _viewModel.Login = FakeAuthService.ValidLogin;
        _viewModel.Password = "wrong";
        _viewModel.LoginCommand.Execute(null);

        _viewModel.Password = FakeAuthService.ValidPassword;
        _viewModel.LoginCommand.Execute(null);

        Assert.True(raised);
        Assert.Equal(string.Empty, _viewModel.ErrorMessage);
        Assert.NotNull(_authService.CurrentOperator);
    }
}
