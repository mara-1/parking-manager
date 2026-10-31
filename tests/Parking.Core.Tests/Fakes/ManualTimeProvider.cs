namespace Parking.Core.Tests.Fakes;

// Часы, которые идут только по команде теста: время въезда и выезда предсказуемо.
public sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    public ManualTimeProvider(DateTime start)
    {
        _now = new DateTimeOffset(start, TimeSpan.Zero);
    }

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow()
    {
        return _now;
    }

    public void Advance(TimeSpan delta)
    {
        _now = _now.Add(delta);
    }
}
