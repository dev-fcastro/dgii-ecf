namespace DgiiEcf.Application.Features.Documents.ValidateDocumentSignature;

/// <summary>
/// Cryptographically verifies the signature of a document with the certificate in its KeyInfo.
/// Certificate expiration and trust chain are not checked.
/// </summary>
public sealed record ValidateDocumentSignatureQuery(string SignedXml);
