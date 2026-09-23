using DgiiEcf.Application.Common.Contracts.IDgiiQueryClient;
using DgiiEcf.Application.Common.Responses.TrackingStatusResponse;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.Queries.Common;
using DgiiEcf.Application.Features.Queries.GetTrackStatus.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.GetTrackStatus;

public sealed class GetTrackStatusHandler : IGetTrackStatusHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly IDgiiQueryClient _client;

    public GetTrackStatusHandler(IAccessTokenProvider tokenProvider, IDgiiQueryClient client)
    {
        _tokenProvider = tokenProvider;
        _client = client;
    }

    public async Task<Result<TrackingStatusResponse>> HandleAsync(GetTrackStatusQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.TrackId))
        {
            return QueryErrors.Required(nameof(query.TrackId));
        }

        return await _tokenProvider.ExecuteAsync(
            null,
            (token, ct) => _client.GetTrackStatusAsync(query.TrackId, token, ct),
            cancellationToken);
    }
}
