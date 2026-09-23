using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Arecf;

/// <summary>
/// Acuse de recibo de un e-CF (ARECF).
/// </summary>
[XmlRoot("ARECF")]
public sealed class Arecf
{
    public DetalleAcusedeRecibo? DetalleAcusedeRecibo { get; set; }
}
