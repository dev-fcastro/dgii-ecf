using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce;

public static class ConvertEcf32ToRfceErrors
{
    public static readonly Error NotAnEcf = new("rfce.not_an_ecf", "El documento no es un e-CF (se esperaba el elemento raíz ECF con Encabezado).");

    public static readonly Error NotConsumo = new("rfce.not_consumo", "El RFCE solo aplica a facturas de consumo (TipoeCF 32).");

    public static readonly Error AmountAboveLimit = new(
        "rfce.amount_above_limit",
        "Las facturas de consumo de RD$250,000.00 o más se envían completas a la DGII, no como RFCE.");
}
