namespace DgiiEcf.Application.Features.Documents.GetSecurityCode;

/// <summary>
/// Security code of a signed document: the first 6 characters of its SignatureValue.
/// </summary>
public sealed record GetSecurityCodeQuery(string SignedXml);
