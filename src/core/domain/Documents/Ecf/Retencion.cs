using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class Retencion
{
    public int? IndicadorAgenteRetencionoPercepcion { get; set; }

    public decimal? MontoITBISRetenido { get; set; }

    public decimal? MontoISRRetenido { get; set; }
}
