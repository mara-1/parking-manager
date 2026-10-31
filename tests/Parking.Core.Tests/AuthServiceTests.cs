using Parking.Core.Services;
using Parking.Core.Tests.Fakes;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "5")]
public class AuthServiceTests
{
    private readonly FakeOperatorRepository _repository = new FakeOperatorRepository();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _repository.Add("operator", "pass123");
        _service = new AuthService(_repository);
    }

    [Fact]
    public void Login_ValidCredentials_ReturnsTrueAndSetsCurrentOperator()
    {
        var result = _service.Login("operator", "pass123");

        Assert.True(result);
        Assert.NotNull(_service.CurrentOperator);
        Assert.Equal("operator", _service.CurrentOperator!.Login);
    }

    [Fact]
    public void Login_WrongPassword_ReturnsFalse()
    {
        Assert.False(_service.Login("operator", "wrong"));
        Assert.Null(_service.CurrentOperator);
    }

    [Fact]
    public void Login_UnknownLogin_ReturnsFalse()
    {
        Assert.False(_service.Login("nobody", "pass123"));
    }

    [Theory]
    [InlineData("", "pass123")]
    [InlineData("   ", "pass123")]
    [InlineData("operator", "")]
    public void Login_EmptyCredentials_ReturnsFalse(string login, string password)
    {
        Assert.False(_service.Login(login, password));
    }

    [Fact]
    public void Logout_AfterLogin_ClearsCurrentOperator()
    {
        _service.Login("operator", "pass123");

        _service.Logout();

        Assert.Null(_service.CurrentOperator);
    }
}
