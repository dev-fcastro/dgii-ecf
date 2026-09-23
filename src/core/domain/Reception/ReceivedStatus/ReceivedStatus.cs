namespace DgiiEcf.Domain.Reception.ReceivedStatus;

/// <summary>
/// Estado del acuse de recibo (ARECF).
/// </summary>
public enum ReceivedStatus
{
    /// <summary>e-CF Recibido.</summary>
    Received = 0,

    /// <summary>e-CF No Recibido.</summary>
    NotReceived = 1,
}
