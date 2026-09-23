using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class SubRecargo
{
    public string? TipoSubRecargo { get; set; }

    public decimal? SubRecargoPorcentaje { get; set; }

    public decimal? MontoSubRecargo { get; set; }
}
