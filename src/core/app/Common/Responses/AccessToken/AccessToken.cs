using System.Globalization;

namespace DgiiEcf.Application.Common.Responses.AccessToken;

/// <summary>
/// Token returned by <c>ValidarSemilla</c>. DGII tokens live about one hour.
/// </summary>
public sealed record AccessToken(string Token, string? Expira, string? Expedido)
{
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(55);

    /// <summary>
    /// Moment the token expires. When DGII does not send a parseable <c>expira</c> the token is
    /// assumed to live <see cref="DefaultLifetime"/> from <paramref name="receivedAt"/>.
    /// </summary>
    public DateTimeOffset ResolveExpiration(DateTimeOffset receivedAt) =>
        DateTimeOffset.TryParse(Expira, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var expiresAt)
            ? expiresAt
            : receivedAt.Add(DefaultLifetime);
}
