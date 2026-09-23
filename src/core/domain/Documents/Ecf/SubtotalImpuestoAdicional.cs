using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class SubtotalImpuestoAdicional
{
    public decimal? SubtotalImpuestoSelectivoConsumoEspecificoPagina { get; set; }

    public decimal? SubtotalImpuestoSelectivoConsumoAdvaloremPagina { get; set; }

    public decimal? SubtotalOtrosImpuesto { get; set; }
}
