namespace DgiiEcf.Domain.Documents.EcfType;

/// <summary>
/// Electronic fiscal receipt types (Tipo de e-CF).
/// </summary>
public enum EcfType
{
    /// <summary>Factura de Crédito Fiscal Electrónica.</summary>
    CreditoFiscal = 31,

    /// <summary>Factura de Consumo Electrónica.</summary>
    Consumo = 32,

    /// <summary>Nota de Débito Electrónica.</summary>
    NotaDebito = 33,

    /// <summary>Nota de Crédito Electrónica.</summary>
    NotaCredito = 34,

    /// <summary>Compras Electrónico.</summary>
    Compras = 41,

    /// <summary>Gastos Menores Electrónico.</summary>
    GastosMenores = 43,

    /// <summary>Regímenes Especiales Electrónico.</summary>
    RegimenesEspeciales = 44,

    /// <summary>Gubernamental Electrónico.</summary>
    Gubernamental = 45,

    /// <summary>Comprobante para Exportaciones Electrónico.</summary>
    Exportaciones = 46,

    /// <summary>Comprobante para Pagos al Exterior Electrónico.</summary>
    PagosExterior = 47,
}
