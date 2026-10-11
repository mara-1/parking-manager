using Parking.Core.Models;
using Parking.Core.Services;
using Parking.Core.Tests.Fakes;

namespace Parking.Core.Tests;

public class ParkingServiceTests
{
    private readonly FakeParkingSpotRepository _repository = new FakeParkingSpotRepository(3);
    private readonly ParkingService _service;

    public ParkingServiceTests()
    {
        _service = new ParkingService(_repository);
    }

    [Fact]
    public void RegisterEntry_FreeSpot_OccupiesSpotWithNormalizedNumber()
    {
        _service.RegisterEntry(1, " а123вс73 ");

        var spot = _repository.GetById(1)!;
        Assert.Equal(SpotStatus.Occupied, spot.Status);
        Assert.Equal("А123ВС73", spot.CarNumber);
        Assert.NotNull(spot.OccupiedSince);
        Assert.Equal(1, _repository.UpdateCount);
    }

    [Fact]
    public void RegisterEntry_OccupiedSpot_Throws()
    {
        _service.RegisterEntry(1, "А123ВС73");

        Assert.Throws<InvalidOperationException>(() => _service.RegisterEntry(1, "В456ОР73"));
    }

    [Fact]
    public void RegisterEntry_CarAlreadyParked_Throws()
    {
        _service.RegisterEntry(1, "А123ВС73");

        Assert.Throws<InvalidOperationException>(() => _service.RegisterEntry(2, "А123ВС73"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RegisterEntry_EmptyCarNumber_Throws(string carNumber)
    {
        Assert.Throws<ArgumentException>(() => _service.RegisterEntry(1, carNumber));
    }

    [Fact]
    public void RegisterExit_OccupiedSpot_FreesSpot()
    {
        _service.RegisterEntry(1, "А123ВС73");

        _service.RegisterExit(1);

        var spot = _repository.GetById(1)!;
        Assert.Equal(SpotStatus.Free, spot.Status);
        Assert.Null(spot.CarNumber);
        Assert.Null(spot.OccupiedSince);
    }

    [Fact]
    public void RegisterExit_FreeSpot_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _service.RegisterExit(1));
    }

    [Fact]
    public void RegisterEntry_UnknownSpot_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _service.RegisterEntry(99, "А123ВС73"));
    }
}
