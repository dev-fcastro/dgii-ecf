using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class CodigosItem
{
    public string? TipoCodigo { get; set; }

    public string? CodigoItem { get; set; }
}
