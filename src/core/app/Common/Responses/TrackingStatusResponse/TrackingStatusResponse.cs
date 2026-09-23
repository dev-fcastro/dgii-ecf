using DgiiEcf.Domain.Tracking.TrackStatus;

namespace DgiiEcf.Application.Common.Responses.TrackingStatusResponse;

/// <summary>
/// Response of <c>consultaresultado/api/Consultas/Estado</c>.
/// </summary>
public sealed record TrackingStatusResponse(
    string? TrackId,
    int Codigo,
    string? Estado,
    string? Rnc,
    string? Encf,
    bool SecuenciaUtilizada,
    string? FechaRecepcion,
    IReadOnlyList<DgiiMessage.DgiiMessage>? Mensajes)
{
    public TrackStatus Status => (TrackStatus)Codigo;
}
