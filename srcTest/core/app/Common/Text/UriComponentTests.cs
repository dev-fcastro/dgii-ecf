using DgiiEcf.Application.Common.Text.JsNumber;
using DgiiEcf.Application.Common.Text.UriComponent;

namespace DgiiEcf.Application.Tests.Common.Text;

public sealed class UriComponentTests
{
    [Theory]
    [InlineData("test+code", "test%2Bcode")]
    [InlineData("14-11-2023 03:05:27", "14-11-2023%2003%3A05%3A27")]
    [InlineData("!~*'()-_.", "!~*'()-_.")]
    [InlineData("ñ", "%C3%B1")]
    [InlineData(null, "")]
    public void Encode_BehavesLikeEncodeUriComponent(string? value, string expected)
    {
        Assert.Equal(expected, UriComponent.Encode(value));
    }

    [Theory]
    [InlineData("180000.00", "180000")]
    [InlineData("180000.50", "180000.5")]
    [InlineData("0", "0")]
    [InlineData("0.10", "0.1")]
    public void Format_BehavesLikeJavaScriptNumberToString(string value, string expected)
    {
        Assert.Equal(expected, JsNumber.Format(decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture)));
    }
}
