namespace DgiiEcf.Application.Features.Reception.SendSummary;

/// <summary>
/// Sends a signed RFCE (e-CF 32 &lt; RD$250,000 summary) to <c>fc.dgii.gov.do</c>.
/// </summary>
public sealed record SendSummaryCommand(string SignedXml, string? FileName = null);
