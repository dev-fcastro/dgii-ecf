using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class IdDoc
{
    public int? TipoeCF { get; set; }

    [XmlElement("eNCF")]
    public string? Encf { get; set; }

    public DateOnly? FechaVencimientoSecuencia { get; set; }

    public int? IndicadorNotaCredito { get; set; }

    public int? IndicadorEnvioDiferido { get; set; }

    public int? IndicadorMontoGravado { get; set; }

    public int? IndicadorServicioTodoIncluido { get; set; }

    public string? TipoIngresos { get; set; }

    public int? TipoPago { get; set; }

    public DateOnly? FechaLimitePago { get; set; }

    public string? TerminoPago { get; set; }

    [XmlArrayItem("FormaDePago")]
    public List<FormaDePago>? TablaFormasPago { get; set; }

    public string? TipoCuentaPago { get; set; }

    public string? NumeroCuentaPago { get; set; }

    public string? BancoPago { get; set; }

    public DateOnly? FechaDesde { get; set; }

    public DateOnly? FechaHasta { get; set; }

    public int? TotalPaginas { get; set; }
}
