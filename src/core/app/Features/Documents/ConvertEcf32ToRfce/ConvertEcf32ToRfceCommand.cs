namespace DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce;

/// <summary>
/// Builds the unsigned RFCE (summary) of a signed e-CF 32 under RD$250,000. Sign the result with root
/// <c>RFCE</c> and send it with SendSummary; keep the full signed e-CF 32 on the issuer side.
/// </summary>
public sealed record ConvertEcf32ToRfceCommand(string SignedEcfXml);

/// <summary>
/// Unsigned RFCE XML and the security code taken from the e-CF signature.
/// </summary>
public sealed record RfceConversion(string Xml, string SecurityCode);
