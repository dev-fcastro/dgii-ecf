using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Rfce;

public sealed class RfceComprador
{
    public string? RNCComprador { get; set; }

    public string? IdentificadorExtranjero { get; set; }

    public string? RazonSocialComprador { get; set; }
}
