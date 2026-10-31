using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "8")]
public class MoneyFormatTests
{
    [Theory]
    [InlineData(0, "0 ₽")]
    [InlineData(60, "60 ₽")]
    [InlineData(1200, "1 200 ₽")]
    [InlineData(126300, "126 300 ₽")]
    public void Format_Amount_ReturnsRublesWithSpaces(int amount, string expected)
    {
        Assert.Equal(expected, MoneyFormat.Format(amount));
    }
}
