namespace DgiiEcf.Application.Features.ServiceStatus.VerifyServiceStatus;

/// <summary>
/// Whether the configured environment is available. Requires the API key issued by DGII.
/// </summary>
public sealed record VerifyServiceStatusQuery(string ApiKey);
