using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Contracts.IDgiiAuthenticationClient;

/// <summary>
/// Seed based authentication against DGII or against a receiver (<paramref name="buyerHost"/>).
/// </summary>
public interface IDgiiAuthenticationClient
{
    Task<Result<string>> GetSeedAsync(string? buyerHost, CancellationToken cancellationToken);

    Task<Result<AccessToken>> ValidateSeedAsync(string signedSeed, string? buyerHost, CancellationToken cancellationToken);
}
