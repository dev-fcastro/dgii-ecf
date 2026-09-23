using DgiiEcf.Signing.Tests.Support;

namespace DgiiEcf.Signing.Tests.Jwt.JwtTokenService;

public sealed class JwtTokenServiceTests
{
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero));

    private Signing.Jwt.JwtTokenService.JwtTokenService CreateService() => new(Samples.SampleCertificateProvider(), _time);

    [Fact]
    public void Issue_ThenValidate_ReturnsClaimsAndNotExpired()
    {
        var service = CreateService();
        var token = service.Issue(new Dictionary<string, object> { ["valor"] = "seed-value" }, TimeSpan.FromHours(1)).Value;

        var validation = service.Validate(token);

        Assert.True(validation.IsSuccess, validation.Error.Message);
        Assert.Equal("seed-value", validation.Value.Claims["valor"]);
        Assert.False(validation.Value.IsExpired);
        Assert.Equal(_time.GetUtcNow().AddHours(1), validation.Value.ExpiresAt);
    }

    [Fact]
    public void Validate_AfterLifetime_IsExpired()
    {
        var service = CreateService();
        var token = service.Issue(new Dictionary<string, object> { ["valor"] = "v" }, TimeSpan.FromHours(1)).Value;

        _time.Advance(TimeSpan.FromHours(2));

        Assert.True(service.Validate(token).Value.IsExpired);
    }

    [Fact]
    public void Validate_TamperedToken_ReturnsInvalidSignature()
    {
        var service = CreateService();
        var token = service.Issue(new Dictionary<string, object> { ["valor"] = "v" }, TimeSpan.FromHours(1)).Value;

        var result = service.Validate(token + "1");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Validate_NotAJwt_ReturnsMalformed()
    {
        Assert.Equal("token.malformed", CreateService().Validate("abc").Error.Code);
    }
}
