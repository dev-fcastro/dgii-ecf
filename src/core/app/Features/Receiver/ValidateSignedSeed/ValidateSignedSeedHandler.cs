using System.Globalization;
using DgiiEcf.Application.Common.Contracts.IJwtTokenService;
using DgiiEcf.Application.Common.Contracts.ISignatureVerifier;
using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Application.Common.Xml.XmlDocumentLoader;
using DgiiEcf.Application.Features.Receiver.Common;
using DgiiEcf.Application.Features.Receiver.ValidateSignedSeed.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Receiver.ValidateSignedSeed;

/// <summary>
/// Unlike the Node package (which only compared the digest), the XML signature is verified
/// cryptographically with the certificate the issuer embedded in KeyInfo.
/// </summary>
public sealed class ValidateSignedSeedHandler : IValidateSignedSeedHandler
{
    public const string ValorClaim = "valor";
    public const string TimestampClaim = "timestamp";

    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

    private readonly ISignatureVerifier _signatureVerifier;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly TimeProvider _timeProvider;

    public ValidateSignedSeedHandler(ISignatureVerifier signatureVerifier, IJwtTokenService jwtTokenService, TimeProvider timeProvider)
    {
        _signatureVerifier = signatureVerifier;
        _jwtTokenService = jwtTokenService;
        _timeProvider = timeProvider;
    }

    public Result<AccessToken> Handle(ValidateSignedSeedCommand command)
    {
        var loaded = XmlDocumentLoader.Load(command.SignedSeedXml);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        if (loaded.Value.Root!.Name.LocalName != "SemillaModel")
        {
            return ReceiverErrors.NotASeed;
        }

        var valor = XmlDocumentLoader.FindFirst(loaded.Value, "valor")?.Value.Trim();
        if (string.IsNullOrEmpty(valor))
        {
            return ReceiverErrors.SeedValueMissing;
        }

        var verification = _signatureVerifier.Verify(command.SignedSeedXml);
        if (verification.IsFailure)
        {
            return verification.Error;
        }

        if (!verification.Value.IsValid)
        {
            return ReceiverErrors.InvalidSeedSignature;
        }

        var issuedAt = _timeProvider.GetUtcNow();
        var claims = new Dictionary<string, object>
        {
            [ValorClaim] = valor,
            [TimestampClaim] = issuedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
        };

        var token = _jwtTokenService.Issue(claims, TokenLifetime);
        if (token.IsFailure)
        {
            return token.Error;
        }

        return new AccessToken(
            token.Value,
            issuedAt.Add(TokenLifetime).ToString("O", CultureInfo.InvariantCulture),
            issuedAt.ToString("O", CultureInfo.InvariantCulture));
    }
}
