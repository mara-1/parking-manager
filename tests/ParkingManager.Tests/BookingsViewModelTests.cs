using Parking.Core.Models;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;
using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
[Trait("Task", "9")]
public class BookingsViewModelTests
{
    private readonly ManualTimeProvider _clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0));
    private readonly ParkingService _parking;
    private readonly BookingsViewModel _viewModel;

    public BookingsViewModelTests()
    {
        var bookings = new FakeBookingRepository();
        _parking = new ParkingService(new FakeCategoryRepository(regular: 5, disabled: 1), new FakeSessionRepository(), new FakeTariffRepository(), bookings, _clock);
        _viewModel = new BookingsViewModel(new BookingService(bookings, _parking, _clock), _clock);
    }

    [Fact]
    public void Constructor_SuggestsArrivalInThirtyMinutes()
    {
        Assert.Equal("09:30", _viewModel.ArrivalText);
        Assert.Empty(_viewModel.Bookings);
    }

    [Fact]
    public void Create_ValidInput_AddsWaitingBookingAndRaisesChanged()
    {
        var changed = false;
        _viewModel.Changed += (sender, args) => changed = true;
        _viewModel.CarNumber = "е777кх73";
        _viewModel.ArrivalText = "10:00";

        _viewModel.CreateCommand.Execute(null);

        var booking = Assert.Single(_viewModel.Bookings);
        Assert.Equal("Е777КХ73", booking.CarNumber);
        Assert.Equal("ожидается, снимется в 10:15", booking.StatusText);
        Assert.Equal("Бронь создана: Е777КХ73 на 10:00.", _viewModel.Message);
        Assert.True(changed);
        Assert.Equal(1, _parking.GetOccupancy().Single(o => o.Category == SpotCategory.Regular).Booked);
    }

    [Fact]
    public void Create_WrongTime_ShowsFormatHint()
    {
        _viewModel.CarNumber = "Е777КХ73";
        _viewModel.ArrivalText = "25:00";

        _viewModel.CreateCommand.Execute(null);

        Assert.Equal("Время прибытия укажите в формате ЧЧ:ММ, например 18:30.", _viewModel.Message);
        Assert.Empty(_viewModel.Bookings);
    }

    [Fact]
    public void Create_DisabledCategory_BooksDisabledPlace()
    {
        _viewModel.IsDisabledCategory = true;
        _viewModel.CarNumber = "К300ТТ73";
        _viewModel.ArrivalText = "11:30";

        _viewModel.CreateCommand.Execute(null);

        Assert.Equal("для инвалидов", Assert.Single(_viewModel.Bookings).CategoryName);
    }

    [Fact]
    public void Cancel_SelectedWaitingBooking_MarksCancelled()
    {
        _viewModel.CarNumber = "Е777КХ73";
        _viewModel.ArrivalText = "10:00";
        _viewModel.CreateCommand.Execute(null);
        _viewModel.SelectedBooking = _viewModel.Bookings[0];

        _viewModel.CancelCommand.Execute(null);

        Assert.Equal("отменена", _viewModel.Bookings[0].StatusText);
        Assert.False(_viewModel.CancelCommand.CanExecute(null));
    }

    [Fact]
    public void Load_AfterNoShow_ShowsRemovedBooking()
    {
        _viewModel.CarNumber = "Е777КХ73";
        _viewModel.ArrivalText = "10:00";
        _viewModel.CreateCommand.Execute(null);
        _clock.Advance(TimeSpan.FromMinutes(80));

        _viewModel.Load();

        Assert.Equal("снята — неявка", _viewModel.Bookings[0].StatusText);
    }
}
