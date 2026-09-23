namespace DgiiEcf.Application.Common.Responses.InquiryStatusResponse;

/// <summary>
/// Response of <c>consultaestado/api/Consultas/Estado</c>. <c>codigo</c>: 0 no encontrado, 1 aceptado, 2 rechazado.
/// </summary>
public sealed record InquiryStatusResponse(
    int Codigo,
    string? Estado,
    string? RncEmisor,
    string? NcfElectronico,
    decimal? MontoTotal,
    decimal? TotalITBIS,
    string? FechaEmision,
    string? FechaFirma,
    string? RncComprador,
    string? CodigoSeguridad,
    string? IdExtranjero);
