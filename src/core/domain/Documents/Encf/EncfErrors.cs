using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Domain.Documents.Encf;

public static class EncfErrors
{
    public static readonly Error Empty = new("encf.empty", "El e-NCF es requerido.");

    public static readonly Error InvalidFormat = new(
        "encf.invalid_format",
        "El e-NCF debe tener el formato E + tipo (2 dígitos) + secuencia (10 dígitos), por ejemplo E310000000001.");

    public static readonly Error UnknownType = new("encf.unknown_type", "El tipo de e-CF indicado en el e-NCF no es válido.");
}
