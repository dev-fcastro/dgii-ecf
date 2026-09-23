using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class FormaDePago
{
    public int? FormaPago { get; set; }

    public decimal? MontoPago { get; set; }
}
