namespace DgiiEcf.Application.Features.Receiver.ValidateSignedSeed;

/// <summary>
/// Validates the seed signed by an issuer (<c>fe/autenticacion/api/validacioncertificado</c>) and issues a
/// one hour RS256 token signed with the receiver certificate.
/// </summary>
public sealed record ValidateSignedSeedCommand(string SignedSeedXml);
