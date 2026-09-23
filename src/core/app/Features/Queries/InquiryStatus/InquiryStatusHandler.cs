using DgiiEcf.Application.Common.Contracts.IDgiiQueryClient;
using DgiiEcf.Application.Common.Responses.InquiryStatusResponse;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.Queries.InquiryStatus.Contracts;
using DgiiEcf.Application.Features.Queries.QueryErrors;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.InquiryStatus;

public sealed class InquiryStatusHandler : IInquiryStatusHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly IDgiiQueryClient _client;

    public InquiryStatusHandler(IAccessTokenProvider tokenProvider, IDgiiQueryClient client)
    {
        _tokenProvider = tokenProvider;
        _client = client;
    }

    public async Task<Result<InquiryStatusResponse>> HandleAsync(InquiryStatusQuery query, CancellationToken cancellationToken = default)
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
            (token, ct) => _client.InquiryStatusAsync(query.RncEmisor, query.Encf, query.RncComprador, query.SecurityCode, token, ct),
            cancellationToken);
    }
}
