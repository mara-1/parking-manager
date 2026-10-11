using System.Globalization;
using System.Windows.Media;
using Parking.Core.Models;
using ParkingManager.Converters;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
public class SpotStatusToBrushConverterTests
{
    private readonly SpotStatusToBrushConverter _converter = new SpotStatusToBrushConverter();

    [Theory]
    [InlineData(SpotStatus.Free, "#FF90EE90")]
    [InlineData(SpotStatus.Occupied, "#FFCD5C5C")]
    [InlineData(SpotStatus.Booked, "#FFFFD700")]
    public void Convert_SpotStatus_ReturnsStatusColor(SpotStatus status, string expectedColor)
    {
        var result = _converter.Convert(status, typeof(Brush), string.Empty, CultureInfo.InvariantCulture);

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
            () => _converter.ConvertBack(Brushes.LightGreen, typeof(SpotStatus), string.Empty, CultureInfo.InvariantCulture));
    }
}
