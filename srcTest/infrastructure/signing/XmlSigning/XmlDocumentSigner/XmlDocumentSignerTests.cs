using System.Xml.Linq;
using DgiiEcf.Signing.Tests.Support;

namespace DgiiEcf.Signing.Tests.XmlSigning.XmlDocumentSigner;

public sealed class XmlDocumentSignerTests
{
    private static readonly XNamespace Dsig = "http://www.w3.org/2000/09/xmldsig#";

    private readonly Signing.XmlSigning.XmlDocumentSigner.XmlDocumentSigner _signer = new(Samples.SampleCertificateProvider());

    [Fact]
    public void Sign_Seed_ProducesTheDigestExpectedByDgii()
    {
        var result = _signer.Sign(Samples.Read("seed.xml"), "SemillaModel");

        Assert.True(result.IsSuccess, result.Error.Message);
        var digest = XDocument.Parse(result.Value).Descendants(Dsig + "DigestValue").Single().Value;
        Assert.Equal("0OGl/9Xvybi3ZVXP9oteBl/m5/dNvx94brb3v7H9QeA=", digest);
    }

    [Fact]
    public void Sign_Seed_UsesTheDgiiXmlDsigProfile()
    {
        var signed = _signer.Sign(Samples.Read("seed.xml"), "SemillaModel").Value;
        var root = XDocument.Parse(signed).Root!;
        var signature = root.Elements().Last();

        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?><SemillaModel", signed);
        Assert.DoesNotContain("\n", signed);
        Assert.Equal(Dsig + "Signature", signature.Name);
        Assert.Equal("http://www.w3.org/TR/2001/REC-xml-c14n-20010315", signature.Descendants(Dsig + "CanonicalizationMethod").Single().Attribute("Algorithm")!.Value);
        Assert.Equal("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", signature.Descendants(Dsig + "SignatureMethod").Single().Attribute("Algorithm")!.Value);
        Assert.Equal(string.Empty, signature.Descendants(Dsig + "Reference").Single().Attribute("URI")!.Value);
        Assert.Equal(
            "http://www.w3.org/2000/09/xmldsig#enveloped-signature",
            signature.Descendants(Dsig + "Transform").Single().Attribute("Algorithm")!.Value);
        Assert.Equal("http://www.w3.org/2001/04/xmlenc#sha256", signature.Descendants(Dsig + "DigestMethod").Single().Attribute("Algorithm")!.Value);
        Assert.Single(signature.Descendants(Dsig + "X509Certificate"));
    }

    [Fact]
    public void Sign_ThenVerify_IsValid()
    {
        var signed = _signer.Sign(Samples.Read("Postulacion.xml"), "Postulacion").Value;

        var verification = new Signing.XmlSigning.SignatureVerifier.SignatureVerifier().Verify(signed);

        Assert.True(verification.IsSuccess, verification.Error.Message);
        Assert.True(verification.Value.IsValid);
        Assert.Contains("<PostulacionID>12345</PostulacionID>", signed);
    }

    [Fact]
    public void Sign_WithoutRootName_UsesTheDocumentRoot()
    {
        var result = _signer.Sign("<CustomDocument><Data>Test</Data></CustomDocument>");

        Assert.True(result.IsSuccess);
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?><CustomDocument><Data>Test</Data><Signature", result.Value);
    }

    [Fact]
    public void Sign_WithCustomRoot_SignsIt()
    {
        var result = _signer.Sign("<MyCustomRoot><Value>1</Value></MyCustomRoot>", "MyCustomRoot");

        Assert.True(result.IsSuccess);
        Assert.True(new Signing.XmlSigning.SignatureVerifier.SignatureVerifier().Verify(result.Value).Value.IsValid);
    }

    [Theory]
    [InlineData("this is not xml at all")]
    [InlineData("<Doc><A></A>")]
    public void Sign_WhenXmlIsInvalid_ReturnsInvalidXml(string xml)
    {
        var result = _signer.Sign(xml);

        Assert.True(result.IsFailure);
        Assert.Equal("signing.invalid_xml", result.Error.Code);
        Assert.Contains("Invalid XML", result.Error.Message);
    }

    [Fact]
    public void Sign_WhenRootDoesNotMatch_ReturnsUnexpectedRoot()
    {
        var result = _signer.Sign("<ECF><A>1</A></ECF>", "RFCE");

        Assert.Equal("signing.unexpected_root", result.Error.Code);
    }

    [Fact]
    public void Sign_WhenAlreadySigned_ReturnsAlreadySigned()
    {
        var signed = _signer.Sign(Samples.Read("seed.xml")).Value;

        var result = _signer.Sign(signed);

        Assert.Equal("signing.already_signed", result.Error.Code);
    }

    [Fact]
    public void Sign_RemovesCommentsAndIndentation()
    {
        var result = _signer.Sign("<Doc>\n  <!-- note -->\n  <A>1</A>\n</Doc>");

        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?><Doc><A>1</A><Signature", result.Value);
    }
}
