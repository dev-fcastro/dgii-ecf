namespace DgiiEcf.Application.Common.Responses.TrackIdSummary;

/// <summary>
/// Item of <c>ConsultaTrackIds/api/TrackIds/Consulta</c>.
/// </summary>
public sealed record TrackIdSummary(string? TrackId, string? Estado, string? FechaRecepcion);
