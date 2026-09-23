using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Signing.Jwt.JwtErrors;

public static class JwtErrors
{
    public static readonly Error Malformed = new("token.malformed", "Token verification failed: el token no tiene el formato JWT.");

    public static readonly Error UnsupportedAlgorithm = new("token.unsupported_algorithm", "Token verification failed: solo se acepta RS256.");

    public static readonly Error InvalidSignature = new("token.invalid_signature", "Token verification failed: invalid signature.");
}
