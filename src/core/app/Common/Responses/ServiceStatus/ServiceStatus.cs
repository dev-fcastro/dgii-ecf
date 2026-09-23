namespace DgiiEcf.Application.Common.Responses.ServiceStatus;

/// <summary>
/// Availability of one DGII service (<c>api/estatusservicios/obtenerestatus</c>).
/// </summary>
public sealed record ServiceStatus(string? Servicio, string? Estatus, string? Ambiente);

/// <summary>
/// Response of <c>api/estatusservicios/obtenerventanasmantenimiento</c>.
/// </summary>
public sealed record MaintenanceResponse(IReadOnlyList<MaintenanceWindow>? VentanaMantenimientos);

public sealed record MaintenanceWindow(string? Ambiente, string? HoraInicio, string? HoraFin, IReadOnlyList<string>? Dias);

/// <summary>
/// Response of <c>api/estatusservicios/verificarestado</c>.
/// </summary>
public sealed record VerificationResponse(string? Estado);
