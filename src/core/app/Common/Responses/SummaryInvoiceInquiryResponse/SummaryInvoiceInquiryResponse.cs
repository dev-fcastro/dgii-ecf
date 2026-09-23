namespace DgiiEcf.Application.Common.Responses.SummaryInvoiceInquiryResponse;

/// <summary>
/// Response of <c>consultarfce/api/Consultas/Consulta</c> (only available in production).
/// </summary>
public sealed record SummaryInvoiceInquiryResponse(
    string? Rnc,
    string? Encf,
    bool SecuenciaUtilizada,
    int Codigo,
    string? Estado,
    IReadOnlyList<DgiiMessage.DgiiMessage>? Mensajes);
