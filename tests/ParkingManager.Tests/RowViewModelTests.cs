using Parking.Core.Models;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

// Строки списков и карточки категорий: тексты, которые видит оператор.
[Trait("Category", "Unit")]
[Trait("Task", "4")]
public class RowViewModelTests
{
    [Theory]
    [Trait("Task", "9")]
    [InlineData(BookingStatus.Waiting, "ожидается, снимется в 10:15", true)]
    [InlineData(BookingStatus.Arrived, "заехал", false)]
    [InlineData(BookingStatus.Cancelled, "отменена", false)]
    [InlineData(BookingStatus.NoShow, "снята — неявка", false)]
    [InlineData((BookingStatus)7, "7", false)]
    public void BookingViewModel_Status_ShowsText(BookingStatus status, string expected, bool isWaiting)
    {
        var booking = new Booking
        {
            Id = 1,
            CarNumber = "Е777КХ73",
            Category = SpotCategory.Disabled,
            ArrivalTime = new DateTime(2026, 10, 24, 10, 0, 0),
            Status = status,
        };

        var row = new BookingViewModel(booking);

        Assert.Equal(expected, row.StatusText);
        Assert.Equal(isWaiting, row.IsWaiting);
        Assert.Equal("10:00", row.ArrivalText);
        Assert.Equal("для инвалидов", row.CategoryName);
    }

    [Theory]
    [Trait("Task", "6")]
    [InlineData(SpotCategory.Regular, "Обычные места", 30)]
    [InlineData(SpotCategory.Disabled, "Места для инвалидов", 10)]
    public void OccupancyViewModel_Category_ShowsTitleAndSchemeWidth(SpotCategory category, string title, int columns)
    {
        var occupancy = new OccupancyViewModel(new OccupancyInfo { Category = category, Capacity = 20, Occupied = 3, Booked = 2 });

        Assert.Equal(title, occupancy.Title);
        Assert.Equal(columns, occupancy.CellColumns);
        Assert.Equal("занято 3 · бронь 2 · свободно 15 из 20", occupancy.Summary);
    }
}
