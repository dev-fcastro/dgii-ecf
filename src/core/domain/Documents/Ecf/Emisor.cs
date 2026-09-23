using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class Emisor
{
    public string? RNCEmisor { get; set; }

    public string? RazonSocialEmisor { get; set; }

    public string? NombreComercial { get; set; }

    public string? Sucursal { get; set; }

    public string? DireccionEmisor { get; set; }

    public string? Municipio { get; set; }

    public string? Provincia { get; set; }

    [XmlArrayItem("TelefonoEmisor")]
    public List<string>? TablaTelefonoEmisor { get; set; }

    public string? CorreoEmisor { get; set; }

    public string? WebSite { get; set; }

    public string? ActividadEconomica { get; set; }

    public string? CodigoVendedor { get; set; }

    public string? NumeroFacturaInterna { get; set; }

    public string? NumeroPedidoInterno { get; set; }

    public string? ZonaVenta { get; set; }

    public string? RutaVenta { get; set; }

    public string? InformacionAdicionalEmisor { get; set; }

    public DateOnly? FechaEmision { get; set; }
}
