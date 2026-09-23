namespace DgiiEcf.Application.Features.ServiceStatus.GetServicesStatus;

/// <summary>
/// Availability of every DGII service. Requires the API key issued by DGII.
/// </summary>
public sealed record GetServicesStatusQuery(string ApiKey);
