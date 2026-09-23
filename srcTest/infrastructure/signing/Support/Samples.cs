using System.Security.Cryptography.X509Certificates;
using DgiiEcf.Signing.Certificates.CertificateLoader;
using DgiiEcf.Signing.Certificates.CertificateProvider;

namespace DgiiEcf.Signing.Tests.Support;

internal static class Samples
{
    public const string SampleCertificatePassword = "pass123";

    public static string PathOf(string fileName) => Path.Combine(AppContext.BaseDirectory, "Samples", fileName);

    public static string Read(string fileName) => File.ReadAllText(PathOf(fileName));

    public static X509Certificate2 SampleCertificate() =>
        CertificateLoader.LoadFromFile(PathOf("SAMPLE_CERT.p12"), SampleCertificatePassword).Value;

    public static StaticCertificateProvider SampleCertificateProvider() => new(SampleCertificate());
}
