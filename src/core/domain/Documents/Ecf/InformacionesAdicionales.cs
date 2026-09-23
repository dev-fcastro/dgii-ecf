using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class InformacionesAdicionales
{
    public DateOnly? FechaEmbarque { get; set; }

    public string? NumeroEmbarque { get; set; }

    public string? NumeroContenedor { get; set; }

    public string? NumeroReferencia { get; set; }

    public string? NombrePuertoEmbarque { get; set; }

    public string? CondicionesEntrega { get; set; }

    public decimal? TotalFob { get; set; }

    public decimal? Seguro { get; set; }

    public decimal? Flete { get; set; }

    public decimal? OtrosGastos { get; set; }

    public decimal? TotalCif { get; set; }

    public string? RegimenAduanero { get; set; }

    public string? NombrePuertoSalida { get; set; }

    public string? NombrePuertoDesembarque { get; set; }

    public decimal? PesoBruto { get; set; }

    public decimal? PesoNeto { get; set; }

    public int? UnidadPesoBruto { get; set; }

    public int? UnidadPesoNeto { get; set; }

    public decimal? CantidadBulto { get; set; }

    public int? UnidadBulto { get; set; }

    public decimal? VolumenBulto { get; set; }

    public int? UnidadVolumen { get; set; }
}
