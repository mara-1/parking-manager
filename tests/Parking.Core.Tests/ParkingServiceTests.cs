using Parking.Core.Models;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;

namespace Parking.Core.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "7")]
public class ParkingServiceTests
{
    private readonly FakeCategoryRepository _categories = new FakeCategoryRepository(regular: 2, disabled: 1);
    private readonly FakeSessionRepository _sessions = new FakeSessionRepository();
    private readonly FakeTariffRepository _tariffs = new FakeTariffRepository(regular: 60m, disabled: 0m);
    private readonly ManualTimeProvider _clock = new ManualTimeProvider(new DateTime(2026, 10, 24, 9, 15, 30));
    private readonly ParkingService _service;

    public ParkingServiceTests()
    {
        _service = new ParkingService(_categories, _sessions, _tariffs, new FakeBookingRepository(), _clock);
    }

    [Fact]
    public void RegisterEntry_ValidNumber_CreatesActiveSessionWithEntryTime()
    {
        var session = _service.RegisterEntry("А123ВС73", SpotCategory.Regular);

        Assert.True(session.IsActive);
        Assert.Equal("А123ВС73", session.CarNumber);
        Assert.Equal(SpotCategory.Regular, session.Category);
        Assert.Equal(new DateTime(2026, 10, 24, 9, 15, 0), session.EntryTime);
        Assert.Equal(1, _sessions.SaveCount);
    }

    [Fact]
    public void RegisterEntry_LatinLettersAndSpaces_StoresNormalizedNumber()
    {
        var session = _service.RegisterEntry(" a 123 bc 73 ", SpotCategory.Regular);

        Assert.Equal("А123ВС73", session.CarNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("А12ВС73")]
    public void RegisterEntry_EmptyOrWrongNumber_Throws(string carNumber)
    {
        Assert.Throws<ParkingOperationException>(() => _service.RegisterEntry(carNumber, SpotCategory.Regular));
        Assert.Empty(_sessions.All);
    }

    [Fact]
    public void RegisterEntry_CarAlreadyParked_Throws()
    {
        _service.RegisterEntry("А123ВС73", SpotCategory.Regular);

        var error = Assert.Throws<ParkingOperationException>(() => _service.RegisterEntry("а123вс73", SpotCategory.Regular));
        Assert.Equal("Автомобиль А123ВС73 уже находится на парковке.", error.Message);
    }

    [Fact]
    [Trait("Task", "6")]
    public void RegisterEntry_NoFreeSpotsInCategory_Throws()
    {
        _service.RegisterEntry("А111АА73", SpotCategory.Regular);
        _service.RegisterEntry("В222ВВ73", SpotCategory.Regular);

        var error = Assert.Throws<ParkingOperationException>(() => _service.RegisterEntry("Е333ЕЕ73", SpotCategory.Regular));
        Assert.Equal("Свободных мест нет (Обычные места).", error.Message);
    }

    [Fact]
    [Trait("Task", "6")]
    public void RegisterEntry_DisabledPlacesFull_DoesNotUseRegularPlaces()
    {
        _service.RegisterEntry("А111АА73", SpotCategory.Disabled);

        Assert.Throws<ParkingOperationException>(() => _service.RegisterEntry("В222ВВ73", SpotCategory.Disabled));
        Assert.Equal(2, _service.GetOccupancy().Single(o => o.Category == SpotCategory.Regular).Free);
    }

    [Fact]
    public void RegisterExit_ActiveSession_SetsExitTime()
    {
        var session = _service.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _clock.Advance(TimeSpan.FromMinutes(95));

        var finished = _service.RegisterExit(session.Id);

        Assert.False(finished.IsActive);
        Assert.Equal(new DateTime(2026, 10, 24, 10, 50, 0), finished.ExitTime);
    }

    [Fact]
    public void RegisterExit_AlreadyFinished_Throws()
    {
        var session = _service.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _service.RegisterExit(session.Id);

        Assert.Throws<ParkingOperationException>(() => _service.RegisterExit(session.Id));
    }

    [Fact]
    public void RegisterExit_UnknownSession_Throws()
    {
        Assert.Throws<ParkingOperationException>(() => _service.RegisterExit(99));
    }

    [Fact]
    public void RegisterEntry_SameCarAfterExit_IsAllowed()
    {
        var first = _service.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _service.RegisterExit(first.Id);

        var second = _service.RegisterEntry("А123ВС73", SpotCategory.Regular);

        Assert.True(second.IsActive);
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    [Trait("Task", "6")]
    public void GetOccupancy_CountsActiveSessionsPerCategory()
    {
        _service.RegisterEntry("А111АА73", SpotCategory.Regular);
        _service.RegisterEntry("В222ВВ73", SpotCategory.Disabled);

        var occupancy = _service.GetOccupancy();

        var regular = occupancy.Single(o => o.Category == SpotCategory.Regular);
        var disabled = occupancy.Single(o => o.Category == SpotCategory.Disabled);
        Assert.Equal((2, 1, 1), (regular.Capacity, regular.Occupied, regular.Free));
        Assert.Equal((1, 1, 0), (disabled.Capacity, disabled.Occupied, disabled.Free));
    }

    [Fact]
    [Trait("Task", "6")]
    public void GetOccupancy_AfterExit_FreesPlace()
    {
        var session = _service.RegisterEntry("А111АА73", SpotCategory.Regular);

        _service.RegisterExit(session.Id);

        Assert.Equal(2, _service.GetOccupancy().Single(o => o.Category == SpotCategory.Regular).Free);
    }

    [Fact]
    public void GetActiveSessions_PartOfNumber_ReturnsMatchingCarsOnly()
    {
        _service.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _service.RegisterEntry("Х777ХХ73", SpotCategory.Regular);

        var found = _service.GetActiveSessions("777");

        Assert.Equal("Х777ХХ73", Assert.Single(found).CarNumber);
        Assert.Equal(2, _service.GetActiveSessions(string.Empty).Count);
    }

    [Fact]
    public void GetHistory_Period_ReturnsActiveAndFinishedSessionsWithEntryInPeriod()
    {
        var first = _service.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _service.RegisterExit(first.Id);
        _clock.Advance(TimeSpan.FromDays(1));
        _service.RegisterEntry("Х777ХХ73", SpotCategory.Regular);

        var day1 = _service.GetHistory(new DateTime(2026, 10, 24), new DateTime(2026, 10, 25), string.Empty);
        var bothDays = _service.GetHistory(new DateTime(2026, 10, 24), new DateTime(2026, 10, 26), string.Empty);

        Assert.Equal("А123ВС73", Assert.Single(day1).CarNumber);
        Assert.Equal(2, bothDays.Count);
    }

    [Fact]
    public void GetHistory_EndBeforeStart_Throws()
    {
        Assert.Throws<ParkingOperationException>(
            () => _service.GetHistory(new DateTime(2026, 10, 25), new DateTime(2026, 10, 24), string.Empty));
    }

    [Fact]
    [Trait("Task", "8")]
    public void RegisterExit_RegularTariff_FixesAmountForStartedHours()
    {
        var session = _service.RegisterEntry("А123ВС73", SpotCategory.Regular);
        _clock.Advance(TimeSpan.FromMinutes(95));

        var finished = _service.RegisterExit(session.Id);

        Assert.Equal(120m, finished.Amount);
    }

    [Fact]
    [Trait("Task", "8")]
    public void RegisterExit_DisabledPlace_IsFree()
    {
        var session = _service.RegisterEntry("М808АА73", SpotCategory.Disabled);
        _clock.Advance(TimeSpan.FromHours(5));

        Assert.Equal(0m, _service.RegisterExit(session.Id).Amount);
    }

    [Fact]
    [Trait("Task", "8")]
    public void CalculateAmount_ActiveSession_GrowsWithTime()
    {
        var session = _service.RegisterEntry("А123ВС73", SpotCategory.Regular);

        var atStart = _service.CalculateAmount(session);
        _clock.Advance(TimeSpan.FromMinutes(61));
        var later = _service.CalculateAmount(session);

        Assert.Equal(60m, atStart);
        Assert.Equal(120m, later);
        Assert.Null(session.Amount);
    }

    [Fact]
    public void RegisterEntry_UnknownCategory_Throws()
    {
        var error = Assert.Throws<ParkingOperationException>(() => _service.RegisterEntry("А123ВС73", (SpotCategory)7));

        Assert.Equal("Категория мест не настроена.", error.Message);
        Assert.Empty(_sessions.All);
    }

    [Fact]
    [Trait("Task", "8")]
    public void CalculateAmount_CategoryWithoutTariff_IsZero()
    {
        var session = new ParkingSession
        {
            CarNumber = "А123ВС73",
            Category = (SpotCategory)7,
            EntryTime = new DateTime(2026, 10, 24, 8, 0, 0),
        };

        Assert.Equal(0m, _service.CalculateAmount(session));
    }
}
