using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Rfce;

public sealed class RfceTotales
{
    public decimal? MontoGravadoTotal { get; set; }

    public decimal? MontoGravadoI1 { get; set; }

    public decimal? MontoGravadoI2 { get; set; }

    public decimal? MontoGravadoI3 { get; set; }

    public decimal? MontoExento { get; set; }

    public decimal? TotalITBIS { get; set; }

    public decimal? TotalITBIS1 { get; set; }

    public decimal? TotalITBIS2 { get; set; }

    public decimal? TotalITBIS3 { get; set; }

    public decimal? MontoImpuestoAdicional { get; set; }

    [XmlArrayItem("ImpuestoAdicional")]
    public List<RfceImpuestoAdicional>? ImpuestosAdicionales { get; set; }

    public decimal? MontoTotal { get; set; }

    public decimal? MontoNoFacturable { get; set; }

    public decimal? MontoPeriodo { get; set; }
}
