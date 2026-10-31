using System.Globalization;
using System.Windows.Media;
using ParkingManager.Converters;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "6")]
public class CellStateToBrushConverterTests
{
    private readonly CellStateToBrushConverter _converter = new CellStateToBrushConverter();

    [Theory]
    [InlineData(CellState.Occupied, "#FFCD5C5C")]
    [InlineData(CellState.Booked, "#FFFFA500")]
    [InlineData(CellState.Free, "#FF90EE90")]
    public void Convert_CellState_ReturnsStateColor(CellState state, string expectedColor)
    {
        var result = _converter.Convert(state, typeof(Brush), string.Empty, CultureInfo.InvariantCulture);

        var brush = Assert.IsAssignableFrom<SolidColorBrush>(result);
        Assert.Equal(expectedColor, brush.Color.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Convert_UnknownValue_ReturnsGray()
    {
        var result = _converter.Convert("unknown", typeof(Brush), string.Empty, CultureInfo.InvariantCulture);

        Assert.Same(Brushes.LightGray, result);
    }

    [Fact]
    public void ConvertBack_Always_Throws()
    {
        Assert.Throws<NotSupportedException>(
            () => _converter.ConvertBack(Brushes.LightGreen, typeof(CellState), string.Empty, CultureInfo.InvariantCulture));
    }
}
