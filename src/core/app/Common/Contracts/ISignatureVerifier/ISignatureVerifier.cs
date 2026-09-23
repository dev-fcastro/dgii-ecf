using System.Security.Cryptography.X509Certificates;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Contracts.ISignatureVerifier;

/// <summary>
/// Verifies the XML-DSig signature of a document using the certificate embedded in its KeyInfo.
/// </summary>
public interface ISignatureVerifier
{
    /// <summary>
    /// Returns the verification outcome. The result only fails when the document cannot be inspected
    /// (invalid XML, missing signature or certificate); a tampered document yields <c>IsValid == false</c>.
    /// </summary>
    Result<SignatureVerification> Verify(string signedXml);
}

/// <summary>
/// Outcome of a signature verification. The certificate chain and expiration are not validated.
/// </summary>
public sealed record SignatureVerification(bool IsValid, X509Certificate2 Certificate, string? DigestValue, string? SignatureValue);
