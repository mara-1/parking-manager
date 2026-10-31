using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
public class DurationFormatTests
{
    [Theory]
    [InlineData(0, "0 мин")]
    [InlineData(45, "45 мин")]
    [InlineData(202, "3 ч 22 мин")]
    [InlineData(1500, "1 д 1 ч")]
    [InlineData(-5, "0 мин")]
    public void Format_Minutes_ReturnsReadableText(int minutes, string expected)
    {
        Assert.Equal(expected, DurationFormat.Format(TimeSpan.FromMinutes(minutes)));
    }
}
