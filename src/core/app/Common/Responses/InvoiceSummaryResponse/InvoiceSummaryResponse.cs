namespace DgiiEcf.Application.Common.Responses.InvoiceSummaryResponse;

/// <summary>
/// Response of <c>recepcionfc/api/recepcion/ecf</c> (RFCE).
/// </summary>
public sealed record InvoiceSummaryResponse(
    int Codigo,
    string? Estado,
    IReadOnlyList<DgiiMessage.DgiiMessage>? Mensajes,
    string? Encf,
    bool SecuenciaUtilizada);
