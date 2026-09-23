using System.Globalization;

namespace DgiiEcf.Application.Common.Time.DominicanClock;

/// <summary>
/// Dominican Republic local time (America/Santo_Domingo, UTC-4 all year, no daylight saving time)
/// and the date formats DGII expects.
/// </summary>
public static class DominicanClock
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(-4);

    public const string DateFormat = "dd-MM-yyyy";

    public const string DateTimeFormat = "dd-MM-yyyy HH:mm:ss";

    public static DateTimeOffset Now(TimeProvider timeProvider) => timeProvider.GetUtcNow().ToOffset(Offset);

    /// <summary>
    /// Current Dominican date and time as <c>dd-MM-yyyy HH:mm:ss</c> (24h).
    /// </summary>
    public static string FormattedDateTime(TimeProvider timeProvider) =>
        Now(timeProvider).ToString(DateTimeFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// Current Dominican date as <c>dd-MM-yyyy</c>.
    /// </summary>
    public static string FormattedDate(TimeProvider timeProvider) =>
        Now(timeProvider).ToString(DateFormat, CultureInfo.InvariantCulture);
}
