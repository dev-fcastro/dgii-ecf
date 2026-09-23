using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class ImpuestoAdicionalOtraMoneda
{
    public string? TipoImpuestoOtraMoneda { get; set; }

    public decimal? TasaImpuestoAdicionalOtraMoneda { get; set; }

    public decimal? MontoImpuestoSelectivoConsumoEspecificoOtraMoneda { get; set; }

    public decimal? MontoImpuestoSelectivoConsumoAdvaloremOtraMoneda { get; set; }

    public decimal? OtrosImpuestosAdicionalesOtraMoneda { get; set; }
}
