using Parking.Core.Services;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "8")]
public class TariffCalculatorTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(60, 1)]
    [InlineData(61, 2)]
    [InlineData(202, 4)]
    [InlineData(1440, 24)]
    public void BillableHours_EveryStartedHourCounts(int minutes, int expectedHours)
    {
        Assert.Equal(expectedHours, TariffCalculator.BillableHours(TimeSpan.FromMinutes(minutes)));
    }

    [Fact]
    public void Calculate_ThreeHoursTwentyTwoMinutes_ChargesFourHours()
    {
        Assert.Equal(240m, TariffCalculator.Calculate(TimeSpan.FromMinutes(202), 60m));
    }

    [Fact]
    public void Calculate_FreeTariff_ReturnsZero()
    {
        Assert.Equal(0m, TariffCalculator.Calculate(TimeSpan.FromHours(10), 0m));
    }
}
