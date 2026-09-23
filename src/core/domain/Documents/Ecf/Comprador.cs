using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class Comprador
{
    public string? RNCComprador { get; set; }

    public string? IdentificadorExtranjero { get; set; }

    public string? RazonSocialComprador { get; set; }

    public string? ContactoComprador { get; set; }

    public string? CorreoComprador { get; set; }

    public string? DireccionComprador { get; set; }

    public string? MunicipioComprador { get; set; }

    public string? ProvinciaComprador { get; set; }

    public string? PaisComprador { get; set; }

    public DateOnly? FechaEntrega { get; set; }

    public string? ContactoEntrega { get; set; }

    public string? DireccionEntrega { get; set; }

    public string? TelefonoAdicional { get; set; }

    public DateOnly? FechaOrdenCompra { get; set; }

    public string? NumeroOrdenCompra { get; set; }

    public string? CodigoInternoComprador { get; set; }

    public string? ResponsablePago { get; set; }

    public string? InformacionAdicionalComprador { get; set; }
}
