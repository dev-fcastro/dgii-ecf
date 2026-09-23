using DgiiEcf.Application.Common.Time.DominicanClock;
using DgiiEcf.Application.Tests.Support;

namespace DgiiEcf.Application.Tests.Common.Time;

public sealed class DominicanClockTests
{
    [Theory]
    [InlineData("2026-08-17T12:15:30Z", "17-08-2026 08:15:30")]
    [InlineData("2026-08-17T17:05:06Z", "17-08-2026 13:05:06")]
    [InlineData("2026-08-17T04:05:06Z", "17-08-2026 00:05:06")]
    [InlineData("2026-08-18T02:30:45Z", "17-08-2026 22:30:45")]
    public void FormattedDateTime_UsesSantoDomingoTimeIn24Hours(string utc, string expected)
    {
        var time = new FakeTimeProvider(DateTimeOffset.Parse(utc, System.Globalization.CultureInfo.InvariantCulture));

        Assert.Equal(expected, DominicanClock.FormattedDateTime(time));
    }

    [Theory]
    [InlineData("2026-08-17T12:15:30Z")]
    [InlineData("2026-08-18T02:30:45Z")]
    public void FormattedDate_UsesSantoDomingoDate(string utc)
    {
        var time = new FakeTimeProvider(DateTimeOffset.Parse(utc, System.Globalization.CultureInfo.InvariantCulture));

        Assert.Equal("17-08-2026", DominicanClock.FormattedDate(time));
    }
}
