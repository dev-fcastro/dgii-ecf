using System.Globalization;
using DgiiEcf.Domain.Documents.EcfType;

namespace DgiiEcf.Domain.Reception.ReceptionRules;

/// <summary>
/// Rules of the issuer-receiver (emisor-receptor) communication standard.
/// </summary>
public static class ReceptionRules
{
    /// <summary>
    /// e-CF types that are never exchanged between taxpayers: 32, 41, 43, 45, 46 and 47.
    /// </summary>
    public static readonly IReadOnlyCollection<EcfType> ExcludedTypes = new HashSet<EcfType>
    {
        EcfType.Consumo,
        EcfType.Compras,
        EcfType.GastosMenores,
        EcfType.Gubernamental,
        EcfType.Exportaciones,
        EcfType.PagosExterior,
    };

    /// <summary>
    /// e-CF types a receiver accepts: 31, 33, 34 and 44.
    /// </summary>
    public static readonly IReadOnlyCollection<EcfType> ReceivableTypes = new HashSet<EcfType>
    {
        EcfType.CreditoFiscal,
        EcfType.NotaDebito,
        EcfType.NotaCredito,
        EcfType.RegimenesEspeciales,
    };

    public static bool IsExcludedType(string? tipoeCf) =>
        int.TryParse(tipoeCf?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var type)
        && ExcludedTypes.Contains((EcfType)type);
}
