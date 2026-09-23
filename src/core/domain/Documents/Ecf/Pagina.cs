using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Ecf;

public sealed class Pagina
{
    public int? PaginaNo { get; set; }

    public int? NoLineaDesde { get; set; }

    public int? NoLineaHasta { get; set; }

    public decimal? SubtotalMontoGravadoPagina { get; set; }

    public decimal? SubtotalMontoGravado1Pagina { get; set; }

    public decimal? SubtotalMontoGravado2Pagina { get; set; }

    public decimal? SubtotalMontoGravado3Pagina { get; set; }

    public decimal? SubtotalExentoPagina { get; set; }

    public decimal? SubtotalItbisPagina { get; set; }

    public decimal? SubtotalItbis1Pagina { get; set; }

    public decimal? SubtotalItbis2Pagina { get; set; }

    public decimal? SubtotalItbis3Pagina { get; set; }

    public decimal? SubtotalImpuestoAdicionalPagina { get; set; }

    public SubtotalImpuestoAdicional? SubtotalImpuestoAdicional { get; set; }

    public decimal? MontoSubtotalPagina { get; set; }

    public decimal? SubtotalMontoNoFacturablePagina { get; set; }
}
