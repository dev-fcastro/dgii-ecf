namespace DgiiEcf.Tests.Support;

internal static class TestEnvironment
{
    public const string SampleCertificatePassword = "pass123";

    public static string SamplePath(string fileName) => Path.Combine(AppContext.BaseDirectory, "Samples", fileName);

    public static string? CertificatePath => Environment.GetEnvironmentVariable("DGII_CERT_PATH");

    public static string? CertificatePassword => Environment.GetEnvironmentVariable("DGII_CERT_PASSWORD");

    public static string? RncEmisor => Environment.GetEnvironmentVariable("DGII_RNC_EMISOR");

    public static bool HasDgiiCredentials =>
        !string.IsNullOrWhiteSpace(CertificatePath) && !string.IsNullOrWhiteSpace(CertificatePassword) && !string.IsNullOrWhiteSpace(RncEmisor);
}

/// <summary>
/// Runs only when <c>DGII_CERT_PATH</c>, <c>DGII_CERT_PASSWORD</c> and <c>DGII_RNC_EMISOR</c> are set: these tests
/// call the real TesteCF environment with a real taxpayer certificate.
/// </summary>
public sealed class DgiiIntegrationFactAttribute : FactAttribute
{
    public DgiiIntegrationFactAttribute()
    {
        if (!TestEnvironment.HasDgiiCredentials)
        {
            Skip = "Integración DGII: defina DGII_CERT_PATH, DGII_CERT_PASSWORD y DGII_RNC_EMISOR para ejecutarla.";
        }
    }
}
