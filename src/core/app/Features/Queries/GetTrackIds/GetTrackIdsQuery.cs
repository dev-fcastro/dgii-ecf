namespace DgiiEcf.Application.Features.Queries.GetTrackIds;

/// <summary>
/// All the trackIds associated with an e-NCF (<c>ConsultaTrackIds/api/TrackIds/Consulta</c>).
/// </summary>
public sealed record GetTrackIdsQuery(string RncEmisor, string Encf);
