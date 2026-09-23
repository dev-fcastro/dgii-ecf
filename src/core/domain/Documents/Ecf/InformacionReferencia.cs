using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class InformacionReferencia
{
    public string? NCFModificado { get; set; }

    public string? RNCOtroContribuyente { get; set; }

    public DateOnly? FechaNCFModificado { get; set; }

    public int? CodigoModificacion { get; set; }

    public string? RazonModificacion { get; set; }
}
