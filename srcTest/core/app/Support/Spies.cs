using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DgiiEcf.Application.Common.Contracts.IAccessTokenStore;
using DgiiEcf.Application.Common.Contracts.IDgiiAuthenticationClient;
using DgiiEcf.Application.Common.Contracts.IDgiiReceptionClient;
using DgiiEcf.Application.Common.Contracts.IJwtTokenService;
using DgiiEcf.Application.Common.Contracts.ISignatureVerifier;
using DgiiEcf.Application.Common.Contracts.IXmlDocumentSigner;
using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Application.Common.Responses.CommercialApprovalResponse;
using DgiiEcf.Application.Common.Responses.InvoiceResponse;
using DgiiEcf.Application.Common.Responses.InvoiceSummaryResponse;
using DgiiEcf.Application.Common.Responses.VoidEncfResponse;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Tests.Support;

internal sealed class AuthenticationClientSpy : IDgiiAuthenticationClient
{
    public Result<string> Seed { get; set; } = "<SemillaModel><valor>abc</valor></SemillaModel>";

    public Result<AccessToken> Token { get; set; } = new AccessToken("token-1", "2026-08-17T13:00:00Z", "2026-08-17T12:00:00Z");

    public List<string?> SeedRequests { get; } = [];

    public List<(string SignedSeed, string? BuyerHost)> Validations { get; } = [];

    public Task<Result<string>> GetSeedAsync(string? buyerHost, CancellationToken cancellationToken)
    {
        SeedRequests.Add(buyerHost);
        return Task.FromResult(Seed);
    }

    public Task<Result<AccessToken>> ValidateSeedAsync(string signedSeed, string? buyerHost, CancellationToken cancellationToken)
    {
        Validations.Add((signedSeed, buyerHost));
        return Task.FromResult(Token);
    }
}

internal sealed class SignerSpy : IXmlDocumentSigner
{
    public List<(string Xml, string? Root)> Calls { get; } = [];

    public Result<string> Sign(string xml, string? rootElementName = null)
    {
        Calls.Add((xml, rootElementName));
        return $"signed:{rootElementName}";
    }
}

internal sealed class AccessTokenStoreSpy : IAccessTokenStore
{
    public Dictionary<string, StoredAccessToken> Tokens { get; } = new();

    public List<string> Removed { get; } = [];

    public StoredAccessToken? Get(string audience) => Tokens.GetValueOrDefault(audience);

    public void Set(string audience, StoredAccessToken token) => Tokens[audience] = token;

    public void Remove(string audience)
    {
        Removed.Add(audience);
        Tokens.Remove(audience);
    }
}

internal sealed class ReceptionClientSpy : IDgiiReceptionClient
{
    public Queue<Result<InvoiceResponse>> InvoiceResponses { get; } = new();

    public List<(string SignedXml, string FileName, string Token, string? BuyerHost)> Sent { get; } = [];

    public Task<Result<InvoiceResponse>> SendElectronicDocumentAsync(
        string signedXml, string fileName, string accessToken, string? buyerHost, CancellationToken cancellationToken)
    {
        Sent.Add((signedXml, fileName, accessToken, buyerHost));
        return Task.FromResult(InvoiceResponses.Count > 0 ? InvoiceResponses.Dequeue() : new InvoiceResponse("track-1", null, null));
    }

    public Task<Result<InvoiceSummaryResponse>> SendSummaryAsync(string signedXml, string fileName, string accessToken, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<Result<CommercialApprovalResponse>> SendCommercialApprovalAsync(
        string signedXml, string fileName, string accessToken, string? buyerHost, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<Result<VoidEncfResponse>> VoidEncfAsync(string signedXml, string fileName, string accessToken, CancellationToken cancellationToken) =>
        throw new NotSupportedException();
}

internal sealed class SignatureVerifierStub : ISignatureVerifier
{
    public bool IsValid { get; set; } = true;

    public List<string> Verified { get; } = [];

    public Result<SignatureVerification> Verify(string signedXml)
    {
        Verified.Add(signedXml);
        using var rsa = RSA.Create(2048);
        var certificate = new CertificateRequest("CN=Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
            .CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        return new SignatureVerification(IsValid, certificate, "digest", "signature");
    }
}

internal sealed class JwtTokenServiceSpy : IJwtTokenService
{
    public List<(IReadOnlyDictionary<string, object> Claims, TimeSpan Lifetime)> Issued { get; } = [];

    public Result<JwtValidation> Validation { get; set; } = new JwtValidation(
        new Dictionary<string, string> { ["valor"] = "v", ["timestamp"] = "t" }, null, null, false);

    public Result<string> Issue(IReadOnlyDictionary<string, object> claims, TimeSpan lifetime)
    {
        Issued.Add((claims, lifetime));
        return "jwt-token";
    }

    public Result<JwtValidation> Validate(string token) => Validation;
}
