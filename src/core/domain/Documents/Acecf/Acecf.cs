using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Acecf;

/// <summary>
/// Aprobación comercial de un e-CF (ACECF).
/// </summary>
[XmlRoot("ACECF")]
public sealed class Acecf
{
    public DetalleAprobacionComercial? DetalleAprobacionComercial { get; set; }
}
