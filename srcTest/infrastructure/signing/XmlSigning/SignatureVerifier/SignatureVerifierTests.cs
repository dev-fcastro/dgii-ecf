using DgiiEcf.Signing.Tests.Support;

namespace DgiiEcf.Signing.Tests.XmlSigning.SignatureVerifier;

public sealed class SignatureVerifierTests
{
    private readonly Signing.XmlSigning.SignatureVerifier.SignatureVerifier _verifier = new();

    [Fact]
    public void Verify_DocumentSignedByDgiiTooling_IsValid()
    {
        var result = _verifier.Verify(Samples.Read("130359334E310000008928.xml"));

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.True(result.Value.IsValid);
        Assert.NotNull(result.Value.Certificate);
    }

    [Fact]
    public void Verify_TamperedDocument_IsNotValid()
    {
        var result = _verifier.Verify(Samples.Read("130359334E310000008928-invalid.xml"));

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.False(result.Value.IsValid);
    }

    [Fact]
    public void Verify_SeedSignedWithBomAndCrLf_IsValid()
    {
        var result = _verifier.Verify(Samples.Read("seed-test_140133.xml"));

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.True(result.Value.IsValid);
    }

    [Fact]
    public void Verify_WithoutSignature_ReturnsSignatureNotFound()
    {
        var result = _verifier.Verify("<ECF><A>1</A></ECF>");

        Assert.Equal("signing.signature_not_found", result.Error.Code);
        Assert.Equal("Signature not found in the XML.", result.Error.Message);
    }
}
