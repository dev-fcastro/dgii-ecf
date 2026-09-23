using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Acecf;

public sealed class DetalleAprobacionComercial
{
    public string Version { get; set; } = "1.0";

    public string? RNCEmisor { get; set; }

    [XmlElement("eNCF")]
    public string? Encf { get; set; }

    public DateOnly? FechaEmision { get; set; }

    public decimal? MontoTotal { get; set; }

    public string? RNCComprador { get; set; }

    public int? Estado { get; set; }

    public string? DetalleMotivoRechazo { get; set; }

    public DateTime? FechaHoraAprobacionComercial { get; set; }
}
