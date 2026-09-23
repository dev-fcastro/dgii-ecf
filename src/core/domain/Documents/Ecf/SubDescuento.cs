using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class SubDescuento
{
    public string? TipoSubDescuento { get; set; }

    public decimal? SubDescuentoPorcentaje { get; set; }

    public decimal? MontoSubDescuento { get; set; }
}
