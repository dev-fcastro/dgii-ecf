using DgiiEcf.Application.Common.Contracts.IDgiiQueryClient;
using DgiiEcf.Application.Common.Responses.TrackIdSummary;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.Queries.GetTrackIds.Contracts;
using DgiiEcf.Application.Features.Queries.QueryErrors;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.GetTrackIds;

public sealed class GetTrackIdsHandler : IGetTrackIdsHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly IDgiiQueryClient _client;

    public GetTrackIdsHandler(IAccessTokenProvider tokenProvider, IDgiiQueryClient client)
    {
        _tokenProvider = tokenProvider;
        _client = client;
    }

    public async Task<Result<IReadOnlyList<TrackIdSummary>>> HandleAsync(GetTrackIdsQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.RncEmisor))
        {
            return QueryErrors.Required(nameof(query.RncEmisor));
        }

        if (string.IsNullOrWhiteSpace(query.Encf))
        {
            return QueryErrors.Required(nameof(query.Encf));
        }

        return await _tokenProvider.ExecuteAsync(
            null,
            (token, ct) => _client.GetTrackIdsAsync(query.RncEmisor, query.Encf, token, ct),
            cancellationToken);
    }
}
