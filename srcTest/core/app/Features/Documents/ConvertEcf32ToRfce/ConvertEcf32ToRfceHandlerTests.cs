using System.Xml.Linq;
using DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce;
using DgiiEcf.Application.Features.Documents.GetSecurityCode;
using DgiiEcf.Application.Tests.Support;

namespace DgiiEcf.Application.Tests.Features.Documents.ConvertEcf32ToRfce;

public sealed class ConvertEcf32ToRfceHandlerTests
{
    private readonly ConvertEcf32ToRfceHandler _handler = new(new GetSecurityCodeHandler());

    [Theory]
    [InlineData("signedECF32.xml", "rfce.xml")]
    [InlineData("signedECF32-without-tax.xml", "rfceWithoutAditionaTax.xml")]
    public void Handle_SignedEcf32_ProducesTheExpectedRfce(string source, string expected)
    {
        var result = _handler.Handle(new ConvertEcf32ToRfceCommand(Samples.Read(source)));

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.Equal(Samples.Normalize(Samples.Read(expected)), result.Value.Xml);
        Assert.Equal("m+tPLr", result.Value.SecurityCode);
    }

    [Fact]
    public void Handle_WithASingleImpuestoAdicional_KeepsIt()
    {
        var ecf = XDocument.Parse(Samples.Normalize(Samples.Read("signedECF32.xml")));
        var impuestos = ecf.Descendants("Totales").Single().Element("ImpuestosAdicionales")!;
        impuestos.Elements("ImpuestoAdicional").Skip(1).Remove();

        var result = _handler.Handle(new ConvertEcf32ToRfceCommand(ecf.ToString()));

        Assert.True(result.IsSuccess, result.Error.Message);
        var rfceImpuesto = Assert.Single(XDocument.Parse(result.Value.Xml).Descendants("ImpuestoAdicional"));
        Assert.Equal("001", rfceImpuesto.Element("TipoImpuesto")!.Value);
        Assert.Null(rfceImpuesto.Element("TasaImpuestoAdicional"));
    }

    [Fact]
    public void Handle_WhenTypeIsNot32_ReturnsNotConsumo()
    {
        var xml = Samples.Normalize(Samples.Read("signedECF32.xml")).Replace("<TipoeCF>32</TipoeCF>", "<TipoeCF>31</TipoeCF>");

        var result = _handler.Handle(new ConvertEcf32ToRfceCommand(xml));

        Assert.Equal(ConvertEcf32ToRfceErrors.NotConsumo, result.Error);
    }

    [Fact]
    public void Handle_WhenAmountIs250000OrMore_ReturnsAmountAboveLimit()
    {
        var ecf = XDocument.Parse(Samples.Normalize(Samples.Read("signedECF32.xml")));
        ecf.Descendants("Totales").Single().Element("MontoTotal")!.Value = "250000.00";

        var result = _handler.Handle(new ConvertEcf32ToRfceCommand(ecf.ToString()));

        Assert.Equal(ConvertEcf32ToRfceErrors.AmountAboveLimit, result.Error);
    }

    [Fact]
    public void Handle_WhenNotSigned_ReturnsSignatureValueNotFound()
    {
        var result = _handler.Handle(new ConvertEcf32ToRfceCommand(
            "<ECF><Encabezado><IdDoc><TipoeCF>32</TipoeCF></IdDoc></Encabezado></ECF>"));

        Assert.Equal(GetSecurityCodeErrors.SignatureValueNotFound, result.Error);
    }

    [Fact]
    public void Handle_WhenNotAnEcf_ReturnsNotAnEcf()
    {
        var result = _handler.Handle(new ConvertEcf32ToRfceCommand("<RFCE><Encabezado /></RFCE>"));

        Assert.Equal(ConvertEcf32ToRfceErrors.NotAnEcf, result.Error);
    }
}
