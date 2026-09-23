using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;

namespace DgiiEcf.Options.DgiiEcfOptions;

/// <summary>
/// Everything the package needs in one place, for code based configuration
/// (<c>services.AddDgiiEcf(options =&gt; ...)</c> or <c>DgiiEcfClient.Create(...)</c>).
/// With <c>IConfiguration</c> the same values are read from the <c>DgiiEcf</c> section and
/// the certificate from <c>DgiiEcf:Certificate</c>.
/// </summary>
public sealed class DgiiEcfOptions
{
    public DgiiEnvironment Environment { get; set; } = DgiiEnvironment.Test;

    /// <summary>Path of the .p12/.pfx. Use this or <see cref="CertificateBase64"/>.</summary>
    public string? CertificatePath { get; set; }

    /// <summary>Content of the .p12/.pfx in base64.</summary>
    public string? CertificateBase64 { get; set; }

    public string? CertificatePassword { get; set; }

    /// <summary>API key of the DGII service status API (optional).</summary>
    public string? StatusApiKey { get; set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    public string EcfBaseUrl { get; set; } = "https://ecf.dgii.gov.do";

    public string FcBaseUrl { get; set; } = "https://fc.dgii.gov.do";

    public string StatusBaseUrl { get; set; } = "https://statusecf.dgii.gov.do";
}
