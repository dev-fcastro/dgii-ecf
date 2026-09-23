using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using DgiiEcf.Application.Common.Contracts.ISignatureVerifier;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Signing.XmlSigning.SignatureVerifier;

/// <summary>
/// Verifies the signature against the certificate embedded in KeyInfo. Certificate expiration and chain are
/// not checked.
/// </summary>
/// <remarks>
/// The document is first verified exactly as received. DGII's own tooling (and most C# signers) sign the
/// document loaded without insignificant whitespace and then save it indented, so when the exact bytes do not
/// verify the document is verified again with whitespace-only nodes removed.
/// </remarks>
public sealed class SignatureVerifier : ISignatureVerifier
{
    public Result<SignatureVerification> Verify(string signedXml)
    {
        if (string.IsNullOrWhiteSpace(signedXml))
        {
            return SigningErrors.SigningErrors.InvalidXml("the document is empty.");
        }

        var exact = SecureXml.SecureXml.Load(signedXml, preserveWhitespace: true);
        if (exact.IsFailure)
        {
            return exact.Error;
        }

        if (FindSignature(exact.Value) is not { } signatureElement)
        {
            return SigningErrors.SigningErrors.SignatureNotFound;
        }

        var certificateText = Text(signatureElement, "X509Certificate");
        if (string.IsNullOrWhiteSpace(certificateText))
        {
            return SigningErrors.SigningErrors.CertificateNotFound;
        }

        X509Certificate2 certificate;
        try
        {
            certificate = Certificates.CertificateLoader.CertificateLoader.LoadPublic(
                Convert.FromBase64String(string.Concat(certificateText.Where(character => !char.IsWhiteSpace(character)))));
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException)
        {
            return SigningErrors.SigningErrors.InvalidEmbeddedCertificate;
        }

        var isValid = Check(exact.Value, signatureElement, certificate);
        if (!isValid)
        {
            var stripped = SecureXml.SecureXml.Load(signedXml, preserveWhitespace: false);
            isValid = stripped.IsSuccess
                && FindSignature(stripped.Value) is { } strippedSignature
                && Check(stripped.Value, strippedSignature, certificate);
        }

        return new SignatureVerification(isValid, certificate, Text(signatureElement, "DigestValue"), Text(signatureElement, "SignatureValue"));
    }

    private static bool Check(XmlDocument document, XmlElement signatureElement, X509Certificate2 certificate)
    {
        try
        {
            var signed = new SignedXml(document);
            signed.LoadXml(signatureElement);
            return signed.CheckSignature(certificate, verifySignatureOnly: true);
        }
        catch (Exception exception) when (exception is CryptographicException or FormatException)
        {
            return false;
        }
    }

    private static XmlElement? FindSignature(XmlDocument document) =>
        document.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl).OfType<XmlElement>().FirstOrDefault();

    private static string? Text(XmlElement parent, string localName) =>
        parent.GetElementsByTagName(localName, SignedXml.XmlDsigNamespaceUrl).OfType<XmlElement>().FirstOrDefault()?.InnerText.Trim();
}
