using DgiiEcf.Application.Features.Documents.GenerateQrCodeUrl;
using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;

namespace DgiiEcf.Application.Tests.Features.Documents.GenerateQrCodeUrl;

public sealed class GenerateQrCodeUrlHandlerTests
{
    private readonly GenerateQrCodeUrlHandler _handler = new();

    private string Fc(string securityCode = "BucMq7", decimal amount = 180000.00m, DgiiEnvironment environment = DgiiEnvironment.Test,
        string rnc = "130862346", string encf = "E310004567002") =>
        _handler.Handle(new FcQrCodeUrlQuery(rnc, encf, amount, securityCode, environment));

    private string Ecf(string encf = "E310004567002", string? buyer = "111111", string securityCode = "BucMq7",
        DgiiEnvironment environment = DgiiEnvironment.Test, string amount = "180000.00") =>
        _handler.Handle(new EcfQrCodeUrlQuery("130862346", buyer, encf, amount, "13-11-2022", "14-11-2023 03:05:27", securityCode, environment));

    [Fact]
    public void HandleFc_Basic_MatchesDgiiFormat()
    {
        Assert.Equal(
            "https://fc.dgii.gov.do/testecf/consultatimbrefc?rncemisor=130862346&encf=E310004567002&montototal=180000&codigoseguridad=BucMq7",
            Fc());
    }

    [Theory]
    [InlineData(DgiiEnvironment.Certification, "certecf")]
    [InlineData(DgiiEnvironment.Production, "ecf")]
    public void HandleFc_Environment_UsesLowercaseSegment(DgiiEnvironment environment, string segment)
    {
        Assert.Equal(
            $"https://fc.dgii.gov.do/{segment}/consultatimbrefc?rncemisor=130862346&encf=E310004567002&montototal=180000&codigoseguridad=BucMq7",
            Fc(environment: environment));
    }

    [Theory]
    [InlineData("Buc+Mq7", "Buc%2BMq7")]
    [InlineData("Buc?Mq7", "Buc%3FMq7")]
    [InlineData("Buc/Mq7", "Buc%2FMq7")]
    [InlineData("Buc:Mq7", "Buc%3AMq7")]
    [InlineData("Buc@Mq7", "Buc%40Mq7")]
    [InlineData("Buc&Mq7", "Buc%26Mq7")]
    [InlineData("Buc=Mq7", "Buc%3DMq7")]
    [InlineData("Buc#Mq7", "Buc%23Mq7")]
    [InlineData("B+u?c/M:q@7&=#", "B%2Bu%3Fc%2FM%3Aq%407%26%3D%23")]
    [InlineData("", "")]
    public void Handle_SecurityCode_IsEncodedLikeEncodeUriComponent(string code, string encoded)
    {
        Assert.EndsWith($"codigoseguridad={encoded}", Fc(code));
        Assert.EndsWith($"codigoseguridad={encoded}", Ecf(securityCode: code));
    }

    [Fact]
    public void HandleFc_OtherParameters_AreEncoded()
    {
        Assert.Contains("rncemisor=130862346%2Btest", Fc(rnc: "130862346+test"));
        Assert.Contains("encf=E310004567002%2Btest", Fc(encf: "E310004567002+test"));
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("180000.50", "180000.5")]
    [InlineData("637.20", "637.2")]
    public void HandleFc_Amount_HasNoTrailingZeros(string amount, string expected)
    {
        Assert.Contains($"montototal={expected}&", Fc(amount: decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void HandleEcf_Basic_MatchesDgiiFormat()
    {
        Assert.Equal(
            "https://ecf.dgii.gov.do/testecf/consultatimbre?rncemisor=130862346&RncComprador=111111&encf=E310004567002&fechaemision=13-11-2022&montototal=180000.00&fechafirma=14-11-2023%2003%3A05%3A27&codigoseguridad=BucMq7",
            Ecf());
    }

    [Theory]
    [InlineData(DgiiEnvironment.Certification, "certecf")]
    [InlineData(DgiiEnvironment.Production, "ecf")]
    public void HandleEcf_Environment_UsesLowercaseSegment(DgiiEnvironment environment, string segment)
    {
        Assert.StartsWith($"https://ecf.dgii.gov.do/{segment}/consultatimbre?rncemisor=130862346&RncComprador=111111&", Ecf(environment: environment));
    }

    [Theory]
    [InlineData("E430004567002")]
    [InlineData("E470004567002")]
    [InlineData("e430004567002")]
    [InlineData("e470004567002")]
    public void HandleEcf_GastosMenoresOrPagosExterior_OmitsBuyer(string encf)
    {
        Assert.DoesNotContain("RncComprador", Ecf(encf));
    }

    [Fact]
    public void HandleEcf_E47_MatchesDgiiFormat()
    {
        Assert.Equal(
            "https://ecf.dgii.gov.do/testecf/consultatimbre?rncemisor=130862346&encf=E470004567002&fechaemision=13-11-2022&montototal=180000.00&fechafirma=14-11-2023%2003%3A05%3A27&codigoseguridad=BucMq7",
            Ecf("E470004567002"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void HandleEcf_EmptyBuyer_OmitsBuyer(string? buyer)
    {
        Assert.Equal(
            "https://ecf.dgii.gov.do/testecf/consultatimbre?rncemisor=130862346&encf=E310004567002&fechaemision=13-11-2022&montototal=180000.00&fechafirma=14-11-2023%2003%3A05%3A27&codigoseguridad=BucMq7",
            Ecf(buyer: buyer));
    }

    [Fact]
    public void HandleEcf_Amount_IsWrittenAsGiven()
    {
        Assert.Contains("montototal=0.00&", Ecf(amount: "0.00"));
    }

    [Fact]
    public void Handle_PlusSign_IsNeverLeftUnencoded()
    {
        Assert.Contains("codigoseguridad=ABC%2B123%2BXYZ", Ecf(securityCode: "ABC+123+XYZ"));
    }
}
