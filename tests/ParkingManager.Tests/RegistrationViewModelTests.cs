using ParkingManager.Tests.Fakes;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
[Trait("Task", "5")]
public class RegistrationViewModelTests
{
    private readonly FakeAuthService _authService = new FakeAuthService { HasOperator = false };
    private readonly RegistrationViewModel _viewModel;

    public RegistrationViewModelTests()
    {
        _viewModel = new RegistrationViewModel(_authService);
    }

    [Fact]
    public void Constructor_NullService_Throws()
    {
        Assert.Throws<ArgumentNullException>("authService", () => new RegistrationViewModel(null!));
    }

    [Fact]
    public void RegisterCommand_EmptyFields_CannotExecute()
    {
        Assert.False(_viewModel.RegisterCommand.CanExecute(null));
    }

    [Theory]
    [InlineData("", "operator", "secret1", "secret1")]
    [InlineData("Аринина Марина", " ", "secret1", "secret1")]
    [InlineData("Аринина Марина", "operator", "", "secret1")]
    [InlineData("Аринина Марина", "operator", "secret1", "")]
    public void RegisterCommand_OneFieldEmpty_CannotExecute(string fullName, string login, string password, string confirm)
    {
        Fill(fullName, login, password, confirm);

        Assert.False(_viewModel.RegisterCommand.CanExecute(null));
    }

    [Fact]
    public void Register_ValidData_RaisesEventAndLogsIn()
    {
        var raised = false;
        _viewModel.RegistrationSucceeded += (sender, args) => raised = true;
        Fill("Аринина Марина", "operator", "secret1", "secret1");

        Assert.True(_viewModel.RegisterCommand.CanExecute(null));
        _viewModel.RegisterCommand.Execute(null);

        Assert.True(raised);
        Assert.Equal(string.Empty, _viewModel.ErrorMessage);
        Assert.Equal("Аринина Марина", _authService.CurrentOperator!.FullName);
    }

    [Fact]
    public void Register_PasswordsDiffer_ShowsErrorAndDoesNotRegister()
    {
        var raised = false;
        _viewModel.RegistrationSucceeded += (sender, args) => raised = true;
        Fill("Аринина Марина", "operator", "secret1", "secret2");

        _viewModel.RegisterCommand.Execute(null);

        Assert.False(raised);
        Assert.Equal("Пароли не совпадают.", _viewModel.ErrorMessage);
        Assert.Null(_authService.CurrentOperator);
    }

    [Fact]
    public void Register_ServiceRejects_ShowsServiceMessage()
    {
        _authService.RegistrationError = "Пароль должен содержать не менее 6 символов.";
        Fill("Аринина Марина", "operator", "123", "123");

        _viewModel.RegisterCommand.Execute(null);

        Assert.Equal("Пароль должен содержать не менее 6 символов.", _viewModel.ErrorMessage);
        Assert.Null(_authService.CurrentOperator);
    }

    [Fact]
    public void Register_AfterError_ClearsErrorWithoutSubscribers()
    {
        Fill("Аринина Марина", "operator", "secret1", "secret2");
        _viewModel.RegisterCommand.Execute(null);

        _viewModel.ConfirmPassword = "secret1";
        _viewModel.RegisterCommand.Execute(null);

        Assert.Equal(string.Empty, _viewModel.ErrorMessage);
        Assert.NotNull(_authService.CurrentOperator);
    }

    private void Fill(string fullName, string login, string password, string confirm)
    {
        _viewModel.FullName = fullName;
        _viewModel.Login = login;
        _viewModel.Password = password;
        _viewModel.ConfirmPassword = confirm;
    }
}
