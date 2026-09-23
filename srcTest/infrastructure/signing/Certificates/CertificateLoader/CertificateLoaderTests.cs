using DgiiEcf.Signing.Certificates.CertificateInfo;
using DgiiEcf.Signing.Tests.Support;

namespace DgiiEcf.Signing.Tests.Certificates.CertificateLoader;

public sealed class CertificateLoaderTests
{
    [Fact]
    public void LoadFromFile_WithPassword_ReturnsCertificateWithPrivateKey()
    {
        var result = Signing.Certificates.CertificateLoader.CertificateLoader.LoadFromFile(Samples.PathOf("SAMPLE_CERT.p12"), Samples.SampleCertificatePassword);

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.True(result.Value.HasPrivateKey);
    }

    [Fact]
    public void LoadFromBase64_WithPassword_ReturnsCertificateWithPrivateKey()
    {
        var base64 = Convert.ToBase64String(File.ReadAllBytes(Samples.PathOf("SAMPLE_CERT.p12")));

        var result = Signing.Certificates.CertificateLoader.CertificateLoader.LoadFromBase64(base64, Samples.SampleCertificatePassword);

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.True(result.Value.HasPrivateKey);
    }

    [Fact]
    public void LoadFromFile_WhenPathDoesNotExist_ReturnsFileNotFound()
    {
        var result = Signing.Certificates.CertificateLoader.CertificateLoader.LoadFromFile("missing.p12", "x");

        Assert.Equal("certificate.file_not_found", result.Error.Code);
    }

    [Fact]
    public void LoadFromFile_WithWrongPassword_ReturnsCannotOpen()
    {
        var result = Signing.Certificates.CertificateLoader.CertificateLoader.LoadFromFile(Samples.PathOf("SAMPLE_CERT.p12"), "wrong");

        Assert.Equal("certificate.cannot_open", result.Error.Code);
    }

    [Fact]
    public void LoadFromBase64_WhenNotBase64_ReturnsInvalidBase64()
    {
        var result = Signing.Certificates.CertificateLoader.CertificateLoader.LoadFromBase64("%%%", "x");

        Assert.Equal("certificate.invalid_base64", result.Error.Code);
    }

    [Fact]
    public void CertificateInfo_From_ExposesSubjectIssuerAndValidity()
    {
        var info = CertificateInfo.From(Samples.SampleCertificate());

        Assert.False(string.IsNullOrEmpty(info.Subject));
        Assert.False(string.IsNullOrEmpty(info.Issuer));
        Assert.False(string.IsNullOrEmpty(info.SerialNumber));
        Assert.True(info.ValidTo > info.ValidFrom);
        Assert.True(info.HasPrivateKey);
    }
}
