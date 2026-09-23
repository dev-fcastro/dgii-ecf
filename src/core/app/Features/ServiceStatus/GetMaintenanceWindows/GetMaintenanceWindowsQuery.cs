namespace DgiiEcf.Application.Features.ServiceStatus.GetMaintenanceWindows;

/// <summary>
/// Scheduled maintenance windows. Requires the API key issued by DGII.
/// </summary>
public sealed record GetMaintenanceWindowsQuery(string ApiKey);
