using Parking.Core.Models;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "6")]
public class SpotCategoryNamesTests
{
    [Theory]
    [InlineData(SpotCategory.Regular, "Обычные места", "обычное")]
    [InlineData(SpotCategory.Disabled, "Места для инвалидов", "для инвалидов")]
    public void Names_KnownCategory_ReturnsRussianTitles(SpotCategory category, string displayName, string shortName)
    {
        Assert.Equal(displayName, category.ToDisplayName());
        Assert.Equal(shortName, category.ToShortName());
    }

    [Fact]
    public void Names_UnknownCategory_FallsBackToNumber()
    {
        var unknown = (SpotCategory)7;

        Assert.Equal("7", unknown.ToDisplayName());
        Assert.Equal("7", unknown.ToShortName());
    }
}
