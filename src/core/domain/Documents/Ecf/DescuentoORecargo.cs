using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class DescuentoORecargo
{
    public int? NumeroLinea { get; set; }

    public string? TipoAjuste { get; set; }

    public int? IndicadorNorma1007 { get; set; }

    public string? DescripcionDescuentooRecargo { get; set; }

    public string? TipoValor { get; set; }

    public decimal? ValorDescuentooRecargo { get; set; }

    public decimal? MontoDescuentooRecargo { get; set; }

    public decimal? MontoDescuentooRecargoOtraMoneda { get; set; }

    public int? IndicadorFacturacionDescuentooRecargo { get; set; }
}
