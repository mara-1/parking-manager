using Parking.Core.Services;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "5")]
public class PasswordHasherTests
{
    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var (hash, salt) = PasswordHasher.Hash("secret");

        Assert.True(PasswordHasher.Verify("secret", hash, salt));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var (hash, salt) = PasswordHasher.Hash("secret");

        Assert.False(PasswordHasher.Verify("Secret", hash, salt));
    }

    [Fact]
    public void Hash_SamePassword_ProducesDifferentSaltAndHash()
    {
        var first = PasswordHasher.Hash("secret");
        var second = PasswordHasher.Hash("secret");

        Assert.NotEqual(first.Salt, second.Salt);
        Assert.NotEqual(first.Hash, second.Hash);
    }
}
