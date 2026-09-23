using System.Security.Cryptography.X509Certificates;

namespace DgiiEcf.Signing.Certificates.CertificateInfo;

/// <summary>
/// Summary of a certificate: subject, issuer, validity and serial number.
/// </summary>
public sealed record CertificateInfo(
    string Subject,
    string Issuer,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    string SerialNumber,
    string Thumbprint,
    bool HasPrivateKey)
{
    public bool IsValidAt(DateTimeOffset moment) => moment >= ValidFrom && moment <= ValidTo;

    public static CertificateInfo From(X509Certificate2 certificate) => new(
        certificate.Subject,
        certificate.Issuer,
        new DateTimeOffset(certificate.NotBefore.ToUniversalTime(), TimeSpan.Zero),
        new DateTimeOffset(certificate.NotAfter.ToUniversalTime(), TimeSpan.Zero),
        certificate.SerialNumber.ToLowerInvariant(),
        certificate.Thumbprint,
        certificate.HasPrivateKey);
}
