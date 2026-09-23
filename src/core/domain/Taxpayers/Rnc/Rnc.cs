using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Domain.Taxpayers.Rnc;

/// <summary>
/// Taxpayer identifier: RNC (9 digits) or cédula (11 digits).
/// </summary>
public sealed record Rnc
{
    private Rnc(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool IsCedula => Value.Length == 11;

    public static Result<Rnc> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return RncErrors.Empty;
        }

        var normalized = value.Trim().Replace("-", string.Empty, StringComparison.Ordinal);
        if ((normalized.Length != 9 && normalized.Length != 11) || !normalized.All(char.IsAsciiDigit))
        {
            return RncErrors.InvalidFormat;
        }

        return new Rnc(normalized);
    }

    public override string ToString() => Value;
}
