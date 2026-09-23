using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

/// <summary>
/// Factura electrónica (e-CF) tipos 31, 32, 33, 34, 41, 43, 44, 45, 46 y 47.
/// </summary>
[XmlRoot("ECF")]
public sealed class Ecf
{
    public EcfEncabezado? Encabezado { get; set; }

    [XmlArrayItem("Item")]
    public List<Item>? DetallesItems { get; set; }

    [XmlArrayItem("Subtotal")]
    public List<Subtotal>? Subtotales { get; set; }

    [XmlArrayItem("DescuentoORecargo")]
    public List<DescuentoORecargo>? DescuentosORecargos { get; set; }

    [XmlArrayItem("Pagina")]
    public List<Pagina>? Paginacion { get; set; }

    public InformacionReferencia? InformacionReferencia { get; set; }

    public DateTime? FechaHoraFirma { get; set; }
}
