using DgiiEcf.Application.Features.Documents.GetSecurityCode;
using DgiiEcf.Application.Tests.Support;

namespace DgiiEcf.Application.Tests.Features.Documents.GetSecurityCode;

public sealed class GetSecurityCodeHandlerTests
{
    private readonly GetSecurityCodeHandler _handler = new();

    [Fact]
    public void Handle_SignedDocument_ReturnsFirstSixCharactersOfSignatureValue()
    {
        var result = _handler.Handle(new GetSecurityCodeQuery(Samples.Read("signedXml.xml")));

        Assert.Equal("gG/XYZ", result.Value.Value);
    }

    [Fact]
    public void Handle_PrefixedSignature_StillFindsTheSignatureValue()
    {
        const string xml = "<ECF><ds:Signature xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"><ds:SignatureValue>\n  ABCDEFGHIJ\n</ds:SignatureValue></ds:Signature></ECF>";

        Assert.Equal("ABCDEF", _handler.Handle(new GetSecurityCodeQuery(xml)).Value.Value);
    }

    [Fact]
    public void Handle_UnsignedDocument_ReturnsSignatureValueNotFound()
    {
        var result = _handler.Handle(new GetSecurityCodeQuery("<ECF><A>1</A></ECF>"));

        Assert.Equal(GetSecurityCodeErrors.SignatureValueNotFound, result.Error);
    }
}
