using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Domain.Documents.SecurityCode;

/// <summary>
/// Security code (Código de seguridad): the first six characters of the document SignatureValue.
/// </summary>
public sealed record SecurityCode
{
    public const int Length = 6;

    private SecurityCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<SecurityCode> Create(string? value)
    {
        if (value is null || value.Length != Length)
        {
            return SecurityCodeErrors.InvalidLength;
        }

        return new SecurityCode(value);
    }

    /// <summary>
    /// Derives the security code from a base64 SignatureValue.
    /// </summary>
    public static Result<SecurityCode> FromSignatureValue(string? signatureValue)
    {
        var trimmed = signatureValue?.Trim();
        if (trimmed is null || trimmed.Length < Length)
        {
            return SecurityCodeErrors.InvalidLength;
        }

        return new SecurityCode(trimmed[..Length]);
    }

    public override string ToString() => Value;
}
