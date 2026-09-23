namespace DgiiEcf.Application.Features.Authentication.AccessTokenAudience;

/// <summary>
/// Key under which a token is stored: DGII itself or a receiver host.
/// </summary>
public static class AccessTokenAudiences
{
    public const string Dgii = "dgii";

    public static string For(string? buyerHost) =>
        string.IsNullOrWhiteSpace(buyerHost) ? Dgii : buyerHost.Trim().TrimEnd('/').ToLowerInvariant();
}
