namespace DgiiEcf.Domain.Tracking.TrackStatus;

/// <summary>
/// Estado de un e-CF según la consulta de resultado (trackId). Values match the DGII <c>codigo</c>.
/// </summary>
public enum TrackStatus
{
    /// <summary>No encontrado.</summary>
    NotFound = 0,

    /// <summary>Aceptado.</summary>
    Accepted = 1,

    /// <summary>Rechazado.</summary>
    Rejected = 2,

    /// <summary>En Proceso.</summary>
    InProcess = 3,

    /// <summary>Aceptado Condicional.</summary>
    ConditionallyAccepted = 4,
}
