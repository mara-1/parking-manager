using Parking.Core.Models;
using Parking.Core.Services;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "10")]
public class ReportTextTests
{
    [Fact]
    public void Period_OneDay_ShowsSingleDate()
    {
        var report = new PeriodReport { From = new DateTime(2026, 10, 24), To = new DateTime(2026, 10, 24) };

        Assert.Equal("24.10.2026", ReportText.Period(report));
    }

    [Fact]
    public void Period_SeveralDays_ShowsRange()
    {
        var report = new PeriodReport { From = new DateTime(2026, 10, 1), To = new DateTime(2026, 10, 31) };

        Assert.Equal("01.10.2026 - 31.10.2026", ReportText.Period(report));
    }

    [Theory]
    [InlineData(0, "-")]
    [InlineData(45, "45 мин")]
    [InlineData(95, "1 ч 35 мин")]
    [InlineData(1570, "26 ч 10 мин")]
    public void Duration_Minutes_FormatsHoursAndMinutes(int minutes, string expected)
    {
        Assert.Equal(expected, ReportText.Duration(TimeSpan.FromMinutes(minutes)));
    }

    [Theory]
    [InlineData(0, "0 руб.")]
    [InlineData(60, "60 руб.")]
    [InlineData(1200.5, "1 200,5 руб.")]
    public void Money_Amount_UsesSpaceSeparatorAndRubles(double amount, string expected)
    {
        Assert.Equal(expected, ReportText.Money((decimal)amount));
    }
}
