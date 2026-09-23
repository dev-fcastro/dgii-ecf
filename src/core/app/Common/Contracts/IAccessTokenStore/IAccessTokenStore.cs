namespace DgiiEcf.Application.Common.Contracts.IAccessTokenStore;

/// <summary>
/// Keeps the access tokens obtained from DGII or from a receiver (buyer host), keyed by audience.
/// </summary>
public interface IAccessTokenStore
{
    StoredAccessToken? Get(string audience);

    void Set(string audience, StoredAccessToken token);

    void Remove(string audience);
}

public sealed record StoredAccessToken(string Token, DateTimeOffset ExpiresAt);
