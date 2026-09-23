using System.Xml.Linq;
using DgiiEcf.Application.Common.Contracts.IJwtTokenService;
using DgiiEcf.Application.Features.Receiver.Common;
using DgiiEcf.Application.Features.Receiver.GenerateSeed;
using DgiiEcf.Application.Features.Receiver.ValidateSignedSeed;
using DgiiEcf.Application.Features.Receiver.ValidateToken;
using DgiiEcf.Application.Tests.Support;

namespace DgiiEcf.Application.Tests.Features.Receiver.ReceiverAuthentication;

public sealed class ReceiverAuthenticationHandlerTests
{
    private const string SignedSeed =
        "<SemillaModel><valor>seed-value</valor><fecha>2026-08-17T08:00:00-04:00</fecha><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignatureValue>x</SignatureValue></Signature></SemillaModel>";

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 8, 17, 12, 0, 0, 123, TimeSpan.Zero));

    [Fact]
    public void GenerateSeed_ReturnsSemillaModelWithRandomValueAndDominicanDate()
    {
        var seed = new GenerateSeedHandler(_time).Handle(new GenerateSeedCommand());

        var root = XDocument.Parse(seed).Root!;
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", seed);
        Assert.Equal("SemillaModel", root.Name.LocalName);
        Assert.Equal(128, Convert.FromBase64String(root.Element("valor")!.Value).Length);
        Assert.Equal("2026-08-17T08:00:00.1230000-04:00", root.Element("fecha")!.Value);
    }

    [Fact]
    public void GenerateSeed_TwoCalls_ProduceDifferentValues()
    {
        var handler = new GenerateSeedHandler(_time);

        Assert.NotEqual(handler.Handle(new GenerateSeedCommand()), handler.Handle(new GenerateSeedCommand()));
    }

    [Fact]
    public void ValidateSignedSeed_ValidSignature_IssuesOneHourToken()
    {
        var verifier = new SignatureVerifierStub();
        var jwt = new JwtTokenServiceSpy();

        var result = new ValidateSignedSeedHandler(verifier, jwt, _time).Handle(new ValidateSignedSeedCommand(SignedSeed));

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.Equal("jwt-token", result.Value.Token);
        var (claims, lifetime) = Assert.Single(jwt.Issued);
        Assert.Equal("seed-value", claims["valor"]);
        Assert.Equal("2026-08-17T12:00:00.123Z", claims["timestamp"]);
        Assert.Equal(TimeSpan.FromHours(1), lifetime);
        Assert.Single(verifier.Verified);
    }

    [Fact]
    public void ValidateSignedSeed_InvalidSignature_DoesNotIssueToken()
    {
        var jwt = new JwtTokenServiceSpy();

        var result = new ValidateSignedSeedHandler(new SignatureVerifierStub { IsValid = false }, jwt, _time)
            .Handle(new ValidateSignedSeedCommand(SignedSeed));

        Assert.Equal(ReceiverErrors.InvalidSeedSignature, result.Error);
        Assert.Empty(jwt.Issued);
    }

    [Fact]
    public void ValidateSignedSeed_NotASeed_ReturnsNotASeed()
    {
        var result = new ValidateSignedSeedHandler(new SignatureVerifierStub(), new JwtTokenServiceSpy(), _time)
            .Handle(new ValidateSignedSeedCommand("<ECF />"));

        Assert.Equal(ReceiverErrors.NotASeed, result.Error);
    }

    [Fact]
    public void ValidateToken_BearerPrefix_IsAccepted()
    {
        var result = new ValidateTokenHandler(new JwtTokenServiceSpy()).Handle(new ValidateTokenQuery("Bearer abc.def.ghi"));

        Assert.True(result.IsSuccess);
        Assert.Equal("v", result.Value.Valor);
        Assert.False(result.Value.IsExpired);
    }

    [Fact]
    public void ValidateToken_Empty_ReturnsEmptyToken()
    {
        Assert.Equal(ReceiverErrors.EmptyToken, new ValidateTokenHandler(new JwtTokenServiceSpy()).Handle(new ValidateTokenQuery(" ")).Error);
    }

    [Fact]
    public void ValidateToken_InvalidSignature_PropagatesTheError()
    {
        var jwt = new JwtTokenServiceSpy { Validation = new Domain.Common.Results.Error("token.invalid_signature", "invalid") };

        Assert.Equal("token.invalid_signature", new ValidateTokenHandler(jwt).Handle(new ValidateTokenQuery("a.b.c")).Error.Code);
    }
}
