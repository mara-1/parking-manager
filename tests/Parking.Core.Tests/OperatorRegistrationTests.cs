using Parking.Core.Services;
using Parking.Core.Tests.Fakes;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "5")]
public class OperatorRegistrationTests
{
    private readonly FakeOperatorRepository _repository = new FakeOperatorRepository();
    private readonly AuthService _service;

    public OperatorRegistrationTests()
    {
        _service = new AuthService(_repository);
    }

    [Fact]
    public void HasOperator_EmptyRepository_ReturnsFalse()
    {
        Assert.False(_service.HasOperator);
    }

    [Fact]
    public void RegisterOperator_ValidData_SavesHashedPasswordAndLogsIn()
    {
        _service.RegisterOperator("  Аринина Марина  ", " operator ", "secret1");

        var account = Assert.Single(_repository.Accounts);
        Assert.Equal("Аринина Марина", account.FullName);
        Assert.Equal("operator", account.Login);
        Assert.NotEqual("secret1", account.PasswordHash);
        Assert.Same(account, _service.CurrentOperator);
        Assert.True(_service.HasOperator);
    }

    [Fact]
    public void RegisterOperator_ThenLogin_AcceptsNewPassword()
    {
        _service.RegisterOperator("Аринина Марина", "operator", "secret1");
        _service.Logout();

        Assert.True(_service.Login("operator", "secret1"));
        Assert.False(_service.Login("operator", "secret2"));
    }

    [Fact]
    public void RegisterOperator_OperatorExists_Throws()
    {
        _repository.Add("operator", "pass123");

        var exception = Assert.Throws<ParkingOperationException>(() => _service.RegisterOperator("Второй", "second", "secret1"));

        Assert.Equal("Оператор уже зарегистрирован. Войдите под своим логином.", exception.Message);
        Assert.Single(_repository.Accounts);
    }

    [Theory]
    [InlineData("", "operator", "secret1", "Укажите ФИО оператора.")]
    [InlineData("Аринина Марина", "  ", "secret1", "Укажите логин.")]
    [InlineData("Аринина Марина", "operator", "12345", "Пароль должен содержать не менее 6 символов.")]
    public void RegisterOperator_InvalidData_ThrowsAndDoesNotSave(string fullName, string login, string password, string expectedMessage)
    {
        var exception = Assert.Throws<ParkingOperationException>(() => _service.RegisterOperator(fullName, login, password));

        Assert.Equal(expectedMessage, exception.Message);
        Assert.Empty(_repository.Accounts);
        Assert.Null(_service.CurrentOperator);
    }
}
