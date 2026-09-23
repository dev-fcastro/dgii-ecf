using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using DgiiEcf.Application.Common.Contracts.ICertificateProvider;
using DgiiEcf.Application.Common.Contracts.IXmlDocumentSigner;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Signing.XmlSigning.XmlDocumentSigner;

/// <summary>
/// Enveloped XML-DSig profile used by DGII:
/// <list type="bullet">
/// <item>CanonicalizationMethod: inclusive C14N <c>http://www.w3.org/TR/2001/REC-xml-c14n-20010315</c>.</item>
/// <item>SignatureMethod: <c>http://www.w3.org/2001/04/xmldsig-more#rsa-sha256</c>.</item>
/// <item>One Reference <c>URI=""</c> with only the enveloped-signature transform and digest <c>http://www.w3.org/2001/04/xmlenc#sha256</c>.</item>
/// <item>KeyInfo/X509Data with the signing certificate only.</item>
/// </list>
/// Whitespace-only nodes and comments are removed before signing and the result is a single line, so the
/// signed bytes are exactly what DGII canonicalizes.
/// </summary>
public sealed class XmlDocumentSigner : IXmlDocumentSigner
{
    public const string Declaration = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

    private readonly ICertificateProvider _certificateProvider;

    public XmlDocumentSigner(ICertificateProvider certificateProvider)
    {
        _certificateProvider = certificateProvider;
    }

    public Result<string> Sign(string xml, string? rootElementName = null)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return SigningErrors.SigningErrors.InvalidXml("the document is empty.");
        }

        var loaded = SecureXml.SecureXml.Load(xml, preserveWhitespace: false);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var document = loaded.Value;
        var root = document.DocumentElement!;
        if (!string.IsNullOrWhiteSpace(rootElementName) && root.LocalName != rootElementName)
        {
            return SigningErrors.SigningErrors.UnexpectedRoot(rootElementName, root.LocalName);
        }

        if (root.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl).Count > 0)
        {
            return SigningErrors.SigningErrors.AlreadySigned;
        }

        var certificate = _certificateProvider.GetCertificate();
        if (certificate.IsFailure)
        {
            return certificate.Error;
        }

        try
        {
            using var privateKey = certificate.Value.GetRSAPrivateKey()
                ?? throw new CryptographicException("the certificate has no RSA private key.");

            var signedXml = new SignedXml(document) { SigningKey = privateKey };
            signedXml.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

            var reference = new Reference(string.Empty) { DigestMethod = SignedXml.XmlDsigSHA256Url };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificate.Value));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();
            root.AppendChild(document.ImportNode(signedXml.GetXml(), deep: true));
        }
        catch (CryptographicException exception)
        {
            return SigningErrors.SigningErrors.SigningFailed(exception.Message);
        }

        return Declaration + root.OuterXml;
    }
}
