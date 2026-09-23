using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class ImpuestoAdicionalTotal
{
    public string? TipoImpuesto { get; set; }

    public decimal? TasaImpuestoAdicional { get; set; }

    public decimal? MontoImpuestoSelectivoConsumoEspecifico { get; set; }

    public decimal? MontoImpuestoSelectivoConsumoAdvalorem { get; set; }

    public decimal? OtrosImpuestosAdicionales { get; set; }
}
