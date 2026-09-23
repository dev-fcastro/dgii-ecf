using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Rfce;

public sealed class RfceEmisor
{
    public string? RNCEmisor { get; set; }

    public string? RazonSocialEmisor { get; set; }

    public DateOnly? FechaEmision { get; set; }
}
