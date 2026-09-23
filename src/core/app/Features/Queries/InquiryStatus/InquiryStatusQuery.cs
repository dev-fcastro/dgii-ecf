namespace DgiiEcf.Application.Features.Queries.InquiryStatus;

/// <summary>
/// Validity of an e-CF (<c>consultaestado/api/Consultas/Estado</c>). For consumo use the 6 character code of the summary; for crédito fiscal the first 6 characters of the SignatureValue.
/// </summary>
public sealed record InquiryStatusQuery(string RncEmisor, string Encf, string? RncComprador = null, string? SecurityCode = null);
