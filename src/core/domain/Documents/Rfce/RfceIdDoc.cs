using System.Xml.Serialization;
using DgiiEcf.Domain.Documents.Ecf;

namespace DgiiEcf.Domain.Documents.Rfce;

public sealed class RfceIdDoc
{
    public int? TipoeCF { get; set; }

    [XmlElement("eNCF")]
    public string? Encf { get; set; }

    public string? TipoIngresos { get; set; }

    public int? TipoPago { get; set; }

    [XmlArrayItem("FormaDePago")]
    public List<FormaDePago>? TablaFormasPago { get; set; }
}
