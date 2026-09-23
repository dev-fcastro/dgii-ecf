using System.Globalization;

namespace DgiiEcf.Application.Common.Text.JsNumber;

/// <summary>
/// Formats a number the way JavaScript <c>Number.prototype.toString()</c> does for the values used
/// in DGII amounts: no trailing zeros (<c>180000.00</c> → <c>180000</c>, <c>180000.50</c> → <c>180000.5</c>).
/// </summary>
public static class JsNumber
{
    public static string Format(decimal value) =>
        value.ToString("0.############################", CultureInfo.InvariantCulture);
}
