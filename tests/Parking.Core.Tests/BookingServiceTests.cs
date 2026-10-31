using Parking.Core.Models;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "9")]
public class BookingServiceTests
{
    private readonly FakeBookingRepository _bookings = new FakeBookingRepository();
    private readonly ManualTimeProvider _clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0));
    private readonly ParkingService _parking;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        var categories = new FakeCategoryRepository(regular: 1, disabled: 1);
        _parking = new ParkingService(categories, new FakeSessionRepository(), new FakeTariffRepository(), _bookings, _clock);
        _service = new BookingService(_bookings, _parking, _clock);
    }

    [Fact]
    public void CreateBooking_FreePlace_HoldsPlaceOfCategory()
    {
        var booking = _service.CreateBooking("е777кх73", SpotCategory.Regular, At(10, 0));

        Assert.Equal("Е777КХ73", booking.CarNumber);
        Assert.Equal(BookingStatus.Waiting, booking.Status);
        Assert.Equal((1, 0), (Regular().Booked, Regular().Free));
    }

    [Fact]
    public void CreateBooking_PastTime_Throws()
    {
        Assert.Throws<ParkingOperationException>(() => _service.CreateBooking("Е777КХ73", SpotCategory.Regular, At(8, 30)));
    }

    [Fact]
    public void CreateBooking_OtherDay_Throws()
    {
        var tomorrow = At(10, 0).AddDays(1);

        Assert.Throws<ParkingOperationException>(() => _service.CreateBooking("Е777КХ73", SpotCategory.Regular, tomorrow));
    }

    [Fact]
    public void CreateBooking_CarAlreadyBooked_Throws()
    {
        _service.CreateBooking("Е777КХ73", SpotCategory.Disabled, At(10, 0));

        var error = Assert.Throws<ParkingOperationException>(() => _service.CreateBooking("Е777КХ73", SpotCategory.Regular, At(11, 0)));
        Assert.Equal("У автомобиля Е777КХ73 уже есть бронь на 10:00.", error.Message);
    }

    [Fact]
    public void CreateBooking_NoFreePlaces_Throws()
    {
        _parking.RegisterEntry("А123ВС73", SpotCategory.Regular);

        Assert.Throws<ParkingOperationException>(() => _service.CreateBooking("Е777КХ73", SpotCategory.Regular, At(10, 0)));
    }

    [Fact]
    public void Entry_CarWithoutBooking_CannotTakeHeldPlace()
    {
        _service.CreateBooking("Е777КХ73", SpotCategory.Regular, At(10, 0));

        var error = Assert.Throws<ParkingOperationException>(() => _parking.RegisterEntry("А123ВС73", SpotCategory.Regular));
        Assert.Equal("Свободных мест нет (Обычные места).", error.Message);
    }

    [Fact]
    public void Entry_BookedCar_TakesHeldPlaceAndMarksArrived()
    {
        var booking = _service.CreateBooking("Е777КХ73", SpotCategory.Regular, At(10, 0));
        _clock.Advance(TimeSpan.FromMinutes(55));

        var session = _parking.RegisterEntry("Е777КХ73", SpotCategory.Regular);

        Assert.Equal(BookingStatus.Arrived, booking.Status);
        Assert.Equal(session.Id, booking.SessionId);
        Assert.Equal((1, 0, 0), (Regular().Occupied, Regular().Booked, Regular().Free));
    }

    [Fact]
    public void Entry_BookedCarWithOtherCategoryChosen_UsesBookingCategory()
    {
        _service.CreateBooking("Е777КХ73", SpotCategory.Disabled, At(10, 0));

        var session = _parking.RegisterEntry("Е777КХ73", SpotCategory.Regular);

        Assert.Equal(SpotCategory.Disabled, session.Category);
    }

    [Fact]
    public void NoShow_FifteenMinutesAfterArrival_BookingStillHeld()
    {
        var booking = _service.CreateBooking("Е777КХ73", SpotCategory.Regular, At(10, 0));
        _clock.Advance(TimeSpan.FromMinutes(75));

        Assert.Equal(1, Regular().Booked);
        Assert.Equal(BookingStatus.Waiting, booking.Status);
    }

    [Fact]
    public void NoShow_SixteenMinutesAfterArrival_BookingRemovedAndPlaceFreed()
    {
        var booking = _service.CreateBooking("Е777КХ73", SpotCategory.Regular, At(10, 0));
        _clock.Advance(TimeSpan.FromMinutes(76));

        Assert.Equal((0, 1), (Regular().Booked, Regular().Free));
        Assert.Equal(BookingStatus.NoShow, booking.Status);
        Assert.Equal("А123ВС73", _parking.RegisterEntry("А123ВС73", SpotCategory.Regular).CarNumber);
    }

    [Fact]
    public void CancelBooking_Waiting_FreesPlace()
    {
        var booking = _service.CreateBooking("Е777КХ73", SpotCategory.Regular, At(10, 0));

        _service.CancelBooking(booking.Id);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(1, Regular().Free);
    }

    [Fact]
    public void CancelBooking_AlreadyArrived_Throws()
    {
        var booking = _service.CreateBooking("Е777КХ73", SpotCategory.Regular, At(10, 0));
        _parking.RegisterEntry("Е777КХ73", SpotCategory.Regular);

        Assert.Throws<ParkingOperationException>(() => _service.CancelBooking(booking.Id));
    }

    [Fact]
    public void GetBookingsForDay_ReturnsBookingsByArrivalTime()
    {
        _service.CreateBooking("Е777КХ73", SpotCategory.Regular, At(18, 0));
        _service.CreateBooking("К300ТТ73", SpotCategory.Disabled, At(11, 30));

        var bookings = _service.GetBookingsForDay(new DateTime(2026, 10, 24));

        Assert.Collection(
            bookings,
            first => Assert.Equal("К300ТТ73", first.CarNumber),
            second => Assert.Equal("Е777КХ73", second.CarNumber));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void CreateBooking_EmptyOrWrongNumber_Throws(string carNumber)
    {
        Assert.Throws<ParkingOperationException>(() => _service.CreateBooking(carNumber, SpotCategory.Regular, At(10, 0)));
        Assert.Empty(_bookings.All);
    }

    [Fact]
    public void CreateBooking_CarAlreadyParked_Throws()
    {
        _parking.RegisterEntry("Е777КХ73", SpotCategory.Regular);

        var error = Assert.Throws<ParkingOperationException>(() => _service.CreateBooking("е777кх73", SpotCategory.Regular, At(10, 0)));

        Assert.Equal("Автомобиль Е777КХ73 уже находится на парковке.", error.Message);
    }

    [Fact]
    public void CreateBooking_UnknownCategory_NoPlaces()
    {
        Assert.Throws<ParkingOperationException>(() => _service.CreateBooking("Е777КХ73", (SpotCategory)7, At(10, 0)));
    }

    [Fact]
    public void CancelBooking_Unknown_Throws()
    {
        var error = Assert.Throws<ParkingOperationException>(() => _service.CancelBooking(42));

        Assert.Equal("Бронь не найдена.", error.Message);
    }

    private static DateTime At(int hour, int minute)
    {
        return new DateTime(2026, 10, 24, hour, minute, 0);
    }

    private OccupancyInfo Regular()
    {
        return _parking.GetOccupancy().Single(o => o.Category == SpotCategory.Regular);
    }
}
