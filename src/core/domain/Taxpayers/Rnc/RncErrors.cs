using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Domain.Taxpayers.Rnc;

public static class RncErrors
{
    public static readonly Error Empty = new("rnc.empty", "El RNC o cédula es requerido.");

    public static readonly Error InvalidFormat = new(
        "rnc.invalid_format",
        "El RNC debe tener 9 dígitos o la cédula 11 dígitos, sin guiones.");
}
