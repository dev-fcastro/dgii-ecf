namespace DgiiEcf.Application.Common.Responses.InvoiceResponse;

/// <summary>
/// Response of <c>recepcion/api/FacturasElectronicas</c> (and of a receiver <c>fe/recepcion/api/ecf</c>).
/// </summary>
public sealed record InvoiceResponse(
    string? TrackId,
    string? Error,
    IReadOnlyList<DgiiMessage.DgiiMessage>? Mensajes);
