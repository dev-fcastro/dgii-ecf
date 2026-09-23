using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Rfce;

public sealed class RfceImpuestoAdicional
{
    public string? TipoImpuesto { get; set; }

    public decimal? MontoImpuestoSelectivoConsumoEspecifico { get; set; }

    public decimal? MontoImpuestoSelectivoConsumoAdvalorem { get; set; }

    public decimal? OtrosImpuestosAdicionales { get; set; }
}
