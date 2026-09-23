using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class EcfEncabezado
{
    public string Version { get; set; } = "1.0";

    public IdDoc? IdDoc { get; set; }

    public Emisor? Emisor { get; set; }

    public Comprador? Comprador { get; set; }

    public InformacionesAdicionales? InformacionesAdicionales { get; set; }

    public Transporte? Transporte { get; set; }

    public Totales? Totales { get; set; }

    public OtraMoneda? OtraMoneda { get; set; }
}
