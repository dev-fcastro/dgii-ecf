namespace DgiiEcf.Domain.Common.Results;

/// <summary>
/// Expected failure described by a stable machine code and a human readable message.
/// </summary>
public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
