using System.Globalization;
using System.Text.RegularExpressions;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Domain.Documents.Encf;

/// <summary>
/// Electronic fiscal receipt number (e-NCF): <c>E</c> + two digit type + ten digit sequence.
/// </summary>
public sealed record Encf
{
    private static readonly Regex Pattern = new(
        "^E(?<type>\\d{2})(?<sequence>\\d{10})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private Encf(string value, EcfType.EcfType type, long sequence)
    {
        Value = value;
        Type = type;
        Sequence = sequence;
    }

    public string Value { get; }

    public EcfType.EcfType Type { get; }

    public long Sequence { get; }

    public static Result<Encf> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return EncfErrors.Empty;
        }

        var normalized = value.Trim().ToUpperInvariant();
        var match = Pattern.Match(normalized);
        if (!match.Success)
        {
            return EncfErrors.InvalidFormat;
        }

        var type = int.Parse(match.Groups["type"].Value, CultureInfo.InvariantCulture);
        if (!Enum.IsDefined(typeof(EcfType.EcfType), type))
        {
            return EncfErrors.UnknownType;
        }

        var sequence = long.Parse(match.Groups["sequence"].Value, CultureInfo.InvariantCulture);
        return new Encf(normalized, (EcfType.EcfType)type, sequence);
    }

    public override string ToString() => Value;
}
