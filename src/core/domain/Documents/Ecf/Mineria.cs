using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class Mineria
{
    public decimal? PesoNetoKilogramo { get; set; }

    public decimal? PesoNetoMineria { get; set; }

    public int? TipoAfiliacion { get; set; }

    public int? Liquidacion { get; set; }
}
