using DgiiEcf.Application.Common.Contracts.IDgiiQueryClient;
using DgiiEcf.Application.Common.Responses.CustomerDirectoryEntry;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.Queries.GetCustomerDirectory.Contracts;
using DgiiEcf.Application.Features.Queries.QueryErrors;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.GetCustomerDirectory;

public sealed class GetCustomerDirectoryHandler : IGetCustomerDirectoryHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly IDgiiQueryClient _client;

    public GetCustomerDirectoryHandler(IAccessTokenProvider tokenProvider, IDgiiQueryClient client)
    {
        _tokenProvider = tokenProvider;
        _client = client;
    }

    public async Task<Result<IReadOnlyList<CustomerDirectoryEntry>>> HandleAsync(GetCustomerDirectoryQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Rnc))
        {
            return QueryErrors.Required(nameof(query.Rnc));
        }

        return await _tokenProvider.ExecuteAsync(
            null,
            (token, ct) => _client.GetCustomerDirectoryAsync(query.Rnc, token, ct),
            cancellationToken);
    }
}
