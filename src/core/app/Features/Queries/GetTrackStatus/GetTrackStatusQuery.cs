namespace DgiiEcf.Application.Features.Queries.GetTrackStatus;

/// <summary>
/// Status of a submission by its trackId (<c>consultaresultado/api/Consultas/Estado</c>).
/// </summary>
public sealed record GetTrackStatusQuery(string TrackId);
