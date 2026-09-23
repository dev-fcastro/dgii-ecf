using DgiiEcf.Application.Common.Contracts.IJwtTokenService;
using DgiiEcf.Application.Features.Receiver.ValidateSignedSeed;
using DgiiEcf.Application.Features.Receiver.ValidateToken.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Receiver.ValidateToken;

public sealed class ValidateTokenHandler : IValidateTokenHandler
{
    private const string BearerPrefix = "Bearer ";

    private readonly IJwtTokenService _jwtTokenService;

    public ValidateTokenHandler(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    public Result<ReceiverTokenInfo> Handle(ValidateTokenQuery query)
    {
        var token = query.Token?.Trim();
        if (token is not null && token.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            token = token[BearerPrefix.Length..].Trim();
        }

        if (string.IsNullOrEmpty(token))
        {
            return ReceiverErrors.ReceiverErrors.EmptyToken;
        }

        var validation = _jwtTokenService.Validate(token);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        var claims = validation.Value.Claims;
        return new ReceiverTokenInfo(
            claims.GetValueOrDefault(ValidateSignedSeedHandler.ValorClaim),
            claims.GetValueOrDefault(ValidateSignedSeedHandler.TimestampClaim),
            validation.Value.IssuedAt,
            validation.Value.ExpiresAt,
            validation.Value.IsExpired);
    }
}
