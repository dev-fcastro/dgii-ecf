namespace DgiiEcf.Domain.Reception.NoReceivedCode;

/// <summary>
/// Código de motivo de no recibido (ARECF).
/// </summary>
public enum NoReceivedCode
{
    /// <summary>Error de especificación.</summary>
    SpecificationError = 1,

    /// <summary>Error de Firma Digital.</summary>
    DigitalSignatureError = 2,

    /// <summary>Envío duplicado.</summary>
    DuplicatedSubmission = 3,

    /// <summary>RNC Comprador no corresponde.</summary>
    BuyerRncMismatch = 4,
}
