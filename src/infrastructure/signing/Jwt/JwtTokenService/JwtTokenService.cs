using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using DgiiEcf.Application.Common.Contracts.ICertificateProvider;
using DgiiEcf.Application.Common.Contracts.IJwtTokenService;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Signing.Jwt.JwtTokenService;

/// <summary>
/// Minimal RS256 JWT (RFC 7519) signed with the taxpayer certificate, without external dependencies.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private const string Algorithm = "RS256";
    private const string IssuedAtClaim = "iat";
    private const string ExpiresClaim = "exp";

    private readonly ICertificateProvider _certificateProvider;
    private readonly TimeProvider _timeProvider;

    public JwtTokenService(ICertificateProvider certificateProvider, TimeProvider timeProvider)
    {
        _certificateProvider = certificateProvider;
        _timeProvider = timeProvider;
    }

    public Result<string> Issue(IReadOnlyDictionary<string, object> claims, TimeSpan lifetime)
    {
        var certificate = _certificateProvider.GetCertificate();
        if (certificate.IsFailure)
        {
            return certificate.Error;
        }

        var now = _timeProvider.GetUtcNow();
        var payload = new Dictionary<string, object>(claims)
        {
            [IssuedAtClaim] = now.ToUnixTimeSeconds(),
            [ExpiresClaim] = now.Add(lifetime).ToUnixTimeSeconds(),
        };

        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, string> { ["alg"] = Algorithm, ["typ"] = "JWT" }));
        var body = Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signingInput = $"{header}.{body}";

        using var privateKey = certificate.Value.GetRSAPrivateKey();
        if (privateKey is null)
        {
            return new Error("token.no_private_key", "El certificado no tiene clave privada RSA para firmar el token.");
        }

        var signature = privateKey.SignData(Encoding.ASCII.GetBytes(signingInput), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return $"{signingInput}.{Base64Url(signature)}";
    }

    public Result<JwtValidation> Validate(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return JwtErrors.JwtErrors.Malformed;
        }

        Dictionary<string, JsonElement>? header;
        Dictionary<string, JsonElement>? payload;
        byte[] signature;
        try
        {
            header = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(FromBase64Url(parts[0]));
            payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(FromBase64Url(parts[1]));
            signature = FromBase64Url(parts[2]);
        }
        catch (Exception exception) when (exception is FormatException or JsonException)
        {
            return JwtErrors.JwtErrors.Malformed;
        }

        if (header is null || payload is null)
        {
            return JwtErrors.JwtErrors.Malformed;
        }

        if (!header.TryGetValue("alg", out var alg) || alg.ValueKind != JsonValueKind.String || alg.GetString() != Algorithm)
        {
            return JwtErrors.JwtErrors.UnsupportedAlgorithm;
        }

        var certificate = _certificateProvider.GetCertificate();
        if (certificate.IsFailure)
        {
            return certificate.Error;
        }

        using var publicKey = certificate.Value.GetRSAPublicKey();
        var signingInput = Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}");
        if (publicKey is null || !publicKey.VerifyData(signingInput, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
        {
            return JwtErrors.JwtErrors.InvalidSignature;
        }

        var issuedAt = ReadTime(payload, IssuedAtClaim);
        var expiresAt = ReadTime(payload, ExpiresClaim);
        var isExpired = expiresAt is null || expiresAt < _timeProvider.GetUtcNow();

        var claims = payload.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ValueKind == JsonValueKind.String ? pair.Value.GetString()! : pair.Value.GetRawText());

        return new JwtValidation(claims, issuedAt, expiresAt, isExpired);
    }

    private static DateTimeOffset? ReadTime(Dictionary<string, JsonElement> payload, string claim) =>
        payload.TryGetValue(claim, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromBase64Url(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '=');
        return Convert.FromBase64String(base64);
    }
}
