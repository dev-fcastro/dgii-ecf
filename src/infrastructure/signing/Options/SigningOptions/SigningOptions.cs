namespace DgiiEcf.Signing.Options.SigningOptions;

/// <summary>
/// Taxpayer certificate (.p12). Provide <see cref="CertificatePath"/> or <see cref="CertificateBase64"/>.
/// Never log or commit the password; load it from a secret store.
/// </summary>
public sealed class SigningOptions
{
    public const string SectionName = "DgiiEcf:Certificate";

    /// <summary>Path of the .p12/.pfx file.</summary>
    public string? CertificatePath { get; set; }

    /// <summary>Content of the .p12/.pfx in base64 (useful for secret managers and containers).</summary>
    public string? CertificateBase64 { get; set; }

    /// <summary>Password of the .p12/.pfx.</summary>
    public string? CertificatePassword { get; set; }
}
