using DgiiEcf.Application.Common.Contracts.IDgiiQueryClient;
using DgiiEcf.Application.Common.Responses.SummaryInvoiceInquiryResponse;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.Queries.Common;
using DgiiEcf.Application.Features.Queries.GetSummaryInvoiceInquiry.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.GetSummaryInvoiceInquiry;

public sealed class GetSummaryInvoiceInquiryHandler : IGetSummaryInvoiceInquiryHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly IDgiiQueryClient _client;

    public GetSummaryInvoiceInquiryHandler(IAccessTokenProvider tokenProvider, IDgiiQueryClient client)
    {
        _tokenProvider = tokenProvider;
        _client = client;
    }

    public async Task<Result<SummaryInvoiceInquiryResponse>> HandleAsync(GetSummaryInvoiceInquiryQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.RncEmisor))
        {
            return QueryErrors.Required(nameof(query.RncEmisor));
        }

        if (string.IsNullOrWhiteSpace(query.Encf))
        {
            return QueryErrors.Required(nameof(query.Encf));
        }

        if (string.IsNullOrWhiteSpace(query.SecurityCode))
        {
            return QueryErrors.Required(nameof(query.SecurityCode));
        }

        return await _tokenProvider.ExecuteAsync(
            null,
            (token, ct) => _client.GetSummaryInvoiceInquiryAsync(query.RncEmisor, query.Encf, query.SecurityCode, token, ct),
            cancellationToken);
    }
}
