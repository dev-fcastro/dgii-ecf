using System.Xml.Serialization;

namespace DgiiEcf.Domain.Documents.Anecf;

public sealed class AnecfEncabezado
{
    public string Version { get; set; } = "1.0";

    public string? RncEmisor { get; set; }

    public int? CantidadeNCFAnulados { get; set; }

    public DateTime? FechaHoraAnulacioneNCF { get; set; }
}
