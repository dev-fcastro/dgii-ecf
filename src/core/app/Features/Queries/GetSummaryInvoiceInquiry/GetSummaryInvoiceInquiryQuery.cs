namespace DgiiEcf.Application.Features.Queries.GetSummaryInvoiceInquiry;

/// <summary>
/// Status of an RFCE (<c>consultarfce/api/Consultas/Consulta</c>). DGII only offers it in production.
/// </summary>
public sealed record GetSummaryInvoiceInquiryQuery(string RncEmisor, string Encf, string SecurityCode);
