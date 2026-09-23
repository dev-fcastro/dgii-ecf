using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class Item
{
    public int? NumeroLinea { get; set; }

    [XmlArrayItem("CodigosItem")]
    public List<CodigosItem>? TablaCodigosItem { get; set; }

    public int? IndicadorFacturacion { get; set; }

    public Retencion? Retencion { get; set; }

    public string? NombreItem { get; set; }

    public int? IndicadorBienoServicio { get; set; }

    public string? DescripcionItem { get; set; }

    public decimal? CantidadItem { get; set; }

    public int? UnidadMedida { get; set; }

    public decimal? CantidadReferencia { get; set; }

    public int? UnidadReferencia { get; set; }

    [XmlArrayItem("SubcantidadItem")]
    public List<SubcantidadItem>? TablaSubcantidad { get; set; }

    public decimal? GradosAlcohol { get; set; }

    public decimal? PrecioUnitarioReferencia { get; set; }

    public DateOnly? FechaElaboracion { get; set; }

    public DateOnly? FechaVencimientoItem { get; set; }

    public Mineria? Mineria { get; set; }

    public decimal? PrecioUnitarioItem { get; set; }

    public decimal? DescuentoMonto { get; set; }

    [XmlArrayItem("SubDescuento")]
    public List<SubDescuento>? TablaSubDescuento { get; set; }

    public decimal? RecargoMonto { get; set; }

    [XmlArrayItem("SubRecargo")]
    public List<SubRecargo>? TablaSubRecargo { get; set; }

    [XmlArrayItem("ImpuestoAdicional")]
    public List<ImpuestoAdicionalItem>? TablaImpuestoAdicional { get; set; }

    public OtraMonedaDetalle? OtraMonedaDetalle { get; set; }

    public decimal? MontoItem { get; set; }
}
