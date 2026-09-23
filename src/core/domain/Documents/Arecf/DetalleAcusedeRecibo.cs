using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Arecf;

public sealed class DetalleAcusedeRecibo
{
    public string Version { get; set; } = "1.0";

    public string? RNCEmisor { get; set; }

    public string? RNCComprador { get; set; }

    [XmlElement("eNCF")]
    public string? Encf { get; set; }

    public int? Estado { get; set; }

    public int? CodigoMotivoNoRecibido { get; set; }

    public DateTime? FechaHoraAcuseRecibo { get; set; }
}
