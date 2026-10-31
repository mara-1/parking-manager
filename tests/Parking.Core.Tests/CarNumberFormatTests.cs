using Parking.Core.Services;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "7")]
public class CarNumberFormatTests
{
    [Theory]
    [InlineData("а123вс73", "А123ВС73")]
    [InlineData(" А 123 ВС 73 ", "А123ВС73")]
    [InlineData("a123bc73", "А123ВС73")]
    [InlineData("Х777ХХ-173", "Х777ХХ173")]
    public void Normalize_DifferentInput_ReturnsUppercaseCyrillicWithoutSpaces(string input, string expected)
    {
        Assert.Equal(expected, CarNumberFormat.Normalize(input));
    }

    [Theory]
    [InlineData("А123ВС73")]
    [InlineData("Е001КХ173")]
    public void IsValid_RussianPlate_ReturnsTrue(string number)
    {
        Assert.True(CarNumberFormat.IsValid(number));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("А12ВС73")]
    [InlineData("Б123ВС73")]
    [InlineData("А123ВС7")]
    public void IsValid_WrongFormat_ReturnsFalse(string number)
    {
        Assert.False(CarNumberFormat.IsValid(number));
    }
}
