using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class Subtotal
{
    public int? NumeroSubTotal { get; set; }

    public string? DescripcionSubtotal { get; set; }

    public int? Orden { get; set; }

    public decimal? SubTotalMontoGravadoTotal { get; set; }

    public decimal? SubTotalMontoGravadoI1 { get; set; }

    public decimal? SubTotalMontoGravadoI2 { get; set; }

    public decimal? SubTotalMontoGravadoI3 { get; set; }

    public decimal? SubTotaITBIS { get; set; }

    public decimal? SubTotaITBIS1 { get; set; }

    public decimal? SubTotaITBIS2 { get; set; }

    public decimal? SubTotaITBIS3 { get; set; }

    public decimal? SubTotalImpuestoAdicional { get; set; }

    public decimal? SubTotalExento { get; set; }

    public decimal? MontoSubTotal { get; set; }

    public int? Lineas { get; set; }
}
