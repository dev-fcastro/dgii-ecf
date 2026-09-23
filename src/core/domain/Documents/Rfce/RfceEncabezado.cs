using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Rfce;

public sealed class RfceEncabezado
{
    public string Version { get; set; } = "1.0";

    public RfceIdDoc? IdDoc { get; set; }

    public RfceEmisor? Emisor { get; set; }

    public RfceComprador? Comprador { get; set; }

    public RfceTotales? Totales { get; set; }

    public string? CodigoSeguridadeCF { get; set; }
}
