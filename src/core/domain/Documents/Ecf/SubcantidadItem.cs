using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class SubcantidadItem
{
    public decimal? Subcantidad { get; set; }

    public int? CodigoSubcantidad { get; set; }
}
