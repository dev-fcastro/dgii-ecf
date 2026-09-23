using System.Collections.Concurrent;
using DgiiEcf.Application.Common.Contracts.IAccessTokenStore;

namespace DgiiEcf.ExternalServices.Authentication.InMemoryAccessTokenStore;

/// <summary>
/// Process-wide token cache. Replace <see cref="IAccessTokenStore"/> with a distributed implementation when
/// several instances should share the DGII token.
/// </summary>
public sealed class InMemoryAccessTokenStore : IAccessTokenStore
{
    private readonly ConcurrentDictionary<string, StoredAccessToken> _tokens = new(StringComparer.OrdinalIgnoreCase);

    public StoredAccessToken? Get(string audience) => _tokens.TryGetValue(audience, out var token) ? token : null;

    public void Set(string audience, StoredAccessToken token) => _tokens[audience] = token;

    public void Remove(string audience) => _tokens.TryRemove(audience, out _);
}
