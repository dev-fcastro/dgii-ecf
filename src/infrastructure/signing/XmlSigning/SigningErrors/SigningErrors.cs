using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Signing.XmlSigning.SigningErrors;

public static class SigningErrors
{
    public static Error InvalidXml(string detail) => new("signing.invalid_xml", $"Invalid XML: {detail}");

    public static Error UnexpectedRoot(string expected, string actual) =>
        new("signing.unexpected_root", $"Se esperaba el elemento raíz '{expected}' pero el documento tiene '{actual}'.");

    public static readonly Error AlreadySigned = new(
        "signing.already_signed",
        "El documento ya contiene una firma (Signature). Firme el documento original sin firma.");

    public static Error SigningFailed(string detail) => new("signing.failed", $"No se pudo firmar el documento: {detail}");

    public static readonly Error SignatureNotFound = new("signing.signature_not_found", "Signature not found in the XML.");

    public static readonly Error CertificateNotFound = new(
        "signing.certificate_not_found",
        "La firma no contiene el certificado (KeyInfo/X509Data/X509Certificate).");

    public static readonly Error InvalidEmbeddedCertificate = new(
        "signing.invalid_embedded_certificate",
        "El certificado incluido en la firma no es válido.");
}
