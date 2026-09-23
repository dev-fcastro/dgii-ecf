using DgiiEcf.Application.Common.Responses.DgiiMessage;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Errors.DgiiApiError;

/// <summary>
/// Failure returned by a DGII (or receiver) web service. <see cref="Error.Message"/> carries the
/// description DGII sent in the body whenever there was one.
/// </summary>
public sealed record DgiiApiError(
    string Code,
    string Message,
    int? Status = null,
    string? StatusText = null,
    string? TransportCode = null,
    string? Resource = null,
    string? Method = null,
    IReadOnlyList<DgiiMessage>? Messages = null,
    string? RawBody = null) : Error(Code, Message)
{
    public const string HttpCodePrefix = "dgii.http_";

    public const string TransportCodeValue = "dgii.transport";

    public const string TimeoutCodeValue = "dgii.timeout";

    public const string InvalidResponseCodeValue = "dgii.invalid_response";
}
