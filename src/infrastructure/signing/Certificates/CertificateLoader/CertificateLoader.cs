using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.Signing.Certificates.CertificateErrors;

namespace DgiiEcf.Signing.Certificates.CertificateLoader;

/// <summary>
/// Opens the taxpayer .p12/.pfx and returns the certificate that owns the private key
/// (not merely the first certificate of the bag).
/// </summary>
public static class CertificateLoader
{
    public static Result<X509Certificate2> LoadFromFile(string path, string? password)
    {
        if (!File.Exists(path))
        {
            return CertificateErrors.CertificateErrors.FileNotFound(path);
        }

        return LoadFromBytes(File.ReadAllBytes(path), password);
    }

    public static Result<X509Certificate2> LoadFromBase64(string base64, string? password)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64.Trim());
        }
        catch (FormatException)
        {
            return CertificateErrors.CertificateErrors.InvalidBase64;
        }

        return LoadFromBytes(bytes, password);
    }

    public static Result<X509Certificate2> LoadFromBytes(byte[] pkcs12, string? password)
    {
        X509Certificate2Collection collection;
        try
        {
#if NET9_0_OR_GREATER
            collection = X509CertificateLoader.LoadPkcs12Collection(pkcs12, password, StorageFlags);
#else
            collection = new X509Certificate2Collection();
            collection.Import(pkcs12, password, StorageFlags);
#endif
        }
        catch (CryptographicException exception)
        {
            return CertificateErrors.CertificateErrors.CannotOpen(exception.Message);
        }

        var certificate = collection.FirstOrDefault(candidate => candidate.HasPrivateKey);
        foreach (var other in collection.Where(candidate => !ReferenceEquals(candidate, certificate)))
        {
            other.Dispose();
        }

        if (certificate is null)
        {
            return CertificateErrors.CertificateErrors.NoPrivateKey;
        }

        using var rsa = certificate.GetRSAPrivateKey();
        if (rsa is null)
        {
            certificate.Dispose();
            return CertificateErrors.CertificateErrors.NotRsa;
        }

        return certificate;
    }

    /// <summary>
    /// Loads a public certificate (DER or base64 DER), e.g. the one embedded in a signature KeyInfo.
    /// </summary>
    public static X509Certificate2 LoadPublic(byte[] der)
    {
#if NET9_0_OR_GREATER
        return X509CertificateLoader.LoadCertificate(der);
#else
        return new X509Certificate2(der);
#endif
    }

    // Keys stay in memory: nothing is written to the user or machine key store.
    private static X509KeyStorageFlags StorageFlags =>
        OperatingSystem.IsMacOS()
            ? X509KeyStorageFlags.Exportable
            : X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet;
}
