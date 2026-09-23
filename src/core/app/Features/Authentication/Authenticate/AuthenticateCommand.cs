namespace DgiiEcf.Application.Features.Authentication.Authenticate;

/// <summary>
/// Authenticates against DGII or, when <paramref name="BuyerHost"/> is set, against a receiver
/// (e.g. <c>https://ecf.dgii.gov.do/Testecf/autenticacion</c> or the buyer's own URL).
/// </summary>
public sealed record AuthenticateCommand(string? BuyerHost = null);
