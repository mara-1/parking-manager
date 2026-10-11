using Parking.Core.Models;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
public class SpotViewModelTests
{
    [Theory]
    [InlineData(SpotStatus.Free, "Свободно")]
    [InlineData(SpotStatus.Occupied, "Занято")]
    [InlineData(SpotStatus.Booked, "Забронировано")]
    public void StatusText_ReturnsRussianStatusName(SpotStatus status, string expected)
    {
        var spot = new SpotViewModel(new ParkingSpot { Id = 1, Number = "A-01", Status = status });

        Assert.Equal(expected, spot.StatusText);
    }

    [Fact]
    public void Details_OccupiedSpot_ShowsCarNumberAndEntryTime()
    {
        var spot = new SpotViewModel(new ParkingSpot
        {
            Id = 1,
            Number = "A-01",
            Status = SpotStatus.Occupied,
            CarNumber = "А123ВС73",
            OccupiedSince = new DateTime(2026, 10, 11, 9, 5, 0, DateTimeKind.Local),
        });

        Assert.Equal("А123ВС73\nс 09:05", spot.Details);
    }

    [Fact]
    public void Details_FreeSpot_ShowsStatusText()
    {
        var spot = new SpotViewModel(new ParkingSpot { Id = 1, Number = "A-01", Status = SpotStatus.Free });

        Assert.Equal("Свободно", spot.Details);
    }
}
