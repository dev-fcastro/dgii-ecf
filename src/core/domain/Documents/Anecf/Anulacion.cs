using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Anecf;

public sealed class Anulacion
{
    public int? NoLinea { get; set; }

    public int? TipoeCF { get; set; }

    [XmlArrayItem("Secuencias")]
    public List<Secuencias>? TablaRangoSecuenciasAnuladaseNCF { get; set; }

    public int? CantidadeNCFAnulados { get; set; }
}
