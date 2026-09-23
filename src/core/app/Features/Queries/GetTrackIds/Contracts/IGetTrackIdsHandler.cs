using DgiiEcf.Application.Common.Responses.TrackIdSummary;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.GetTrackIds.Contracts;

public interface IGetTrackIdsHandler
{
    Task<Result<IReadOnlyList<TrackIdSummary>>> HandleAsync(GetTrackIdsQuery query, CancellationToken cancellationToken = default);
}
