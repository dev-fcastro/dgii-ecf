using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Anecf;

/// <summary>
/// Anulación de secuencias de e-NCF (ANECF).
/// </summary>
[XmlRoot("ANECF")]
public sealed class Anecf
{
    public AnecfEncabezado? Encabezado { get; set; }

    [XmlArrayItem("Anulacion")]
    public List<Anulacion>? DetalleAnulacion { get; set; }
}
