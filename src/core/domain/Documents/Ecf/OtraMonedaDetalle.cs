using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class OtraMonedaDetalle
{
    public decimal? PrecioOtraMoneda { get; set; }

    public decimal? DescuentoOtraMoneda { get; set; }

    public decimal? RecargoOtraMoneda { get; set; }

    public decimal? MontoItemOtraMoneda { get; set; }
}
