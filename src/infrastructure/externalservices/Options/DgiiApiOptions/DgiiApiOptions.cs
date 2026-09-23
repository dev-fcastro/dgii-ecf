using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;

namespace DgiiEcf.ExternalServices.Options.DgiiApiOptions;

/// <summary>
/// DGII web services configuration (section <c>DgiiEcf</c>).
/// </summary>
public sealed class DgiiApiOptions
{
    public const string SectionName = "DgiiEcf";

    /// <summary>Test (TesteCF), Certification (CerteCF) or Production (eCF).</summary>
    public DgiiEnvironment Environment { get; set; } = DgiiEnvironment.Test;

    /// <summary>Host of the e-CF services.</summary>
    public string EcfBaseUrl { get; set; } = "https://ecf.dgii.gov.do";

    /// <summary>Host of the consumo invoice summary (RFCE) services.</summary>
    public string FcBaseUrl { get; set; } = "https://fc.dgii.gov.do";

    /// <summary>Host of the service status API.</summary>
    public string StatusBaseUrl { get; set; } = "https://statusecf.dgii.gov.do";

    /// <summary>API key issued by DGII for the service status API (optional).</summary>
    public string? StatusApiKey { get; set; }

    /// <summary>HTTP timeout per request.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
}
