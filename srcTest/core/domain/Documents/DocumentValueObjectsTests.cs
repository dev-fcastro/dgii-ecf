using DgiiEcf.Domain.Documents.EcfType;
using DgiiEcf.Domain.Documents.Encf;
using DgiiEcf.Domain.Documents.SecurityCode;
using DgiiEcf.Domain.Taxpayers.Rnc;

namespace DgiiEcf.Domain.Tests.Documents;

public sealed class DocumentValueObjectsTests
{
    [Theory]
    [InlineData("E310000000001", EcfType.CreditoFiscal, 1)]
    [InlineData(" e320005000101 ", EcfType.Consumo, 5000101)]
    [InlineData("E470000000123", EcfType.PagosExterior, 123)]
    public void EncfCreate_ValidValue_ExposesTypeAndSequence(string value, EcfType type, long sequence)
    {
        var encf = Encf.Create(value).Value;

        Assert.Equal(type, encf.Type);
        Assert.Equal(sequence, encf.Sequence);
        Assert.Equal(value.Trim().ToUpperInvariant(), encf.Value);
    }

    [Theory]
    [InlineData("", "encf.empty")]
    [InlineData("B0100000001", "encf.invalid_format")]
    [InlineData("E31000000001", "encf.invalid_format")]
    [InlineData("E990000000001", "encf.unknown_type")]
    public void EncfCreate_InvalidValue_ReturnsError(string value, string code)
    {
        Assert.Equal(code, Encf.Create(value).Error.Code);
    }

    [Theory]
    [InlineData("130862346", false)]
    [InlineData("001-1234567-8", true)]
    public void RncCreate_ValidValue_NormalizesIt(string value, bool isCedula)
    {
        var rnc = Rnc.Create(value).Value;

        Assert.Equal(isCedula, rnc.IsCedula);
        Assert.DoesNotContain("-", rnc.Value);
    }

    [Theory]
    [InlineData(null, "rnc.empty")]
    [InlineData("12345", "rnc.invalid_format")]
    [InlineData("13086234A", "rnc.invalid_format")]
    public void RncCreate_InvalidValue_ReturnsError(string? value, string code)
    {
        Assert.Equal(code, Rnc.Create(value).Error.Code);
    }

    [Fact]
    public void SecurityCodeFromSignatureValue_TakesTheFirstSixCharacters()
    {
        Assert.Equal("gG/XYZ", SecurityCode.FromSignatureValue("gG/XYZabc==").Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public void SecurityCodeFromSignatureValue_TooShort_ReturnsInvalidLength(string? value)
    {
        Assert.Equal(SecurityCodeErrors.InvalidLength, SecurityCode.FromSignatureValue(value).Error);
    }
}
