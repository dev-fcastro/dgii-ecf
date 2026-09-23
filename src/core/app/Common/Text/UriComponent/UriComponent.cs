using System.Globalization;
using System.Text;

namespace DgiiEcf.Application.Common.Text.UriComponent;

/// <summary>
/// Same semantics as JavaScript <c>encodeURIComponent</c>: everything except
/// <c>A-Z a-z 0-9 - _ . ! ~ * ' ( )</c> is percent-encoded as UTF-8.
/// <see cref="Uri.EscapeDataString(string)"/> differs because it also escapes <c>! * ' ( )</c>.
/// </summary>
public static class UriComponent
{
    public static string Encode(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var @byte in Encoding.UTF8.GetBytes(value))
        {
            var character = (char)@byte;
            if (IsUnreserved(character))
            {
                builder.Append(character);
            }
            else
            {
                builder.Append('%').Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        return builder.ToString();
    }

    private static bool IsUnreserved(char character) =>
        character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9'
            or '-' or '_' or '.' or '!' or '~' or '*' or '\'' or '(' or ')';
}
