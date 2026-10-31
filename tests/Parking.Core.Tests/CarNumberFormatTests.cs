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

    [Fact]
    public void Normalize_AllLatinLookalikes_BecomeRussianLetters()
    {
        Assert.Equal("АВЕКМНОРСТУХ", CarNumberFormat.Normalize("abekmhopctyx"));
    }

    [Fact]
    public void IsValid_Null_ReturnsFalse()
    {
        Assert.False(CarNumberFormat.IsValid(null));
    }

    [Fact]
    public void RequireValid_LatinInput_ReturnsNormalizedNumber()
    {
        Assert.Equal("Е777КХ173", CarNumberFormat.RequireValid(" e 777 kx 173 "));
    }

    [Theory]
    [InlineData("", "Не указан номер автомобиля.")]
    [InlineData(" - ", "Не указан номер автомобиля.")]
    [InlineData("А12ВС73", "Номер автомобиля указан неверно. Пример: А123ВС73.")]
    [InlineData("П123ВС73", "Номер автомобиля указан неверно. Пример: А123ВС73.")]
    public void RequireValid_EmptyOrWrongNumber_ThrowsWithMessage(string input, string message)
    {
        var error = Assert.Throws<ParkingOperationException>(() => CarNumberFormat.RequireValid(input));

        Assert.Equal(message, error.Message);
    }
}
