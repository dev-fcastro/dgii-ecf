using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Rfce;

/// <summary>
/// Resumen de Factura de Consumo Electrónica (RFCE) para e-CF 32 menores de RD$250,000.
/// </summary>
[XmlRoot("RFCE")]
public sealed class Rfce
{
    public RfceEncabezado? Encabezado { get; set; }
}
