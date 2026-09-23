using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Signing.Certificates.CertificateErrors;

public static class CertificateErrors
{
    public static readonly Error NotConfigured = new(
        "certificate.not_configured",
        "No se configuró el certificado digital (.p12): indique la ruta o el contenido en base64 y la contraseña.");

    public static Error FileNotFound(string path) =>
        new("certificate.file_not_found", $"No se encontró el certificado en '{path}'.");

    public static readonly Error InvalidBase64 = new("certificate.invalid_base64", "El contenido del certificado no es base64 válido.");

    public static Error CannotOpen(string detail) =>
        new("certificate.cannot_open", $"No se pudo abrir el certificado .p12 (¿contraseña incorrecta?): {detail}");

    public static readonly Error NoPrivateKey = new(
        "certificate.no_private_key",
        "El archivo .p12 no contiene un certificado con clave privada.");

    public static readonly Error NotRsa = new("certificate.not_rsa", "La clave del certificado debe ser RSA.");
}
