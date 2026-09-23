using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class OtraMoneda
{
    public string? TipoMoneda { get; set; }

    public decimal? TipoCambio { get; set; }

    public decimal? MontoGravadoTotalOtraMoneda { get; set; }

    public decimal? MontoGravado1OtraMoneda { get; set; }

    public decimal? MontoGravado2OtraMoneda { get; set; }

    public decimal? MontoGravado3OtraMoneda { get; set; }

    public decimal? MontoExentoOtraMoneda { get; set; }

    public decimal? TotalITBISOtraMoneda { get; set; }

    public decimal? TotalITBIS1OtraMoneda { get; set; }

    public decimal? TotalITBIS2OtraMoneda { get; set; }

    public decimal? TotalITBIS3OtraMoneda { get; set; }

    public decimal? MontoImpuestoAdicionalOtraMoneda { get; set; }

    [XmlArrayItem("ImpuestoAdicionalOtraMoneda")]
    public List<ImpuestoAdicionalOtraMoneda>? ImpuestosAdicionalesOtraMoneda { get; set; }

    public decimal? MontoTotalOtraMoneda { get; set; }
}
