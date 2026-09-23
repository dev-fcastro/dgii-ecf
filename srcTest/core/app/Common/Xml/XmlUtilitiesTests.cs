using System.Xml.Linq;
using DgiiEcf.Application.Common.Xml.XmlBodyExtractor;
using DgiiEcf.Application.Common.Xml.XmlValueEditor;
using DgiiEcf.Application.Tests.Support;

namespace DgiiEcf.Application.Tests.Common.Xml;

public sealed class XmlUtilitiesTests
{
    [Fact]
    public void Extract_CommercialApprovalBody_ReturnsTheExactXml()
    {
        var xml = XmlBodyExtractor.Extract(Samples.Read("commercial_approval_raw_response.txt"));

        Assert.NotNull(xml);
        Assert.StartsWith("<?xml version=\"1.0\"?><ACECF xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", xml);
        Assert.Contains("<eNCF>E450000000001</eNCF>", xml);
        Assert.Contains("<DigestValue>yOO0Jm/g9RuAT/ayc31xP++7jmjb6gjYNNeXxE7na4I=</DigestValue>", xml);
        Assert.EndsWith("</Signature></ACECF>", xml);
    }

    [Fact]
    public void Extract_WithoutMatch_ReturnsNull()
    {
        Assert.Null(XmlBodyExtractor.Extract("no xml here"));
    }

    [Fact]
    public void SetValue_ReplacesTheFirstElementText()
    {
        var result = XmlValueEditor.SetValue(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><RFCE><Encabezado><CodigoSeguridadeCF>old</CodigoSeguridadeCF></Encabezado></RFCE>",
            "CodigoSeguridadeCF",
            "1231231");

        Assert.True(result.IsSuccess);
        Assert.Equal("1231231", XDocument.Parse(result.Value).Descendants("CodigoSeguridadeCF").Single().Value);
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?><RFCE>", result.Value);
    }

    [Fact]
    public void SetValue_MissingElement_ReturnsElementNotFound()
    {
        Assert.Equal("xml.element_not_found", XmlValueEditor.SetValue("<A />", "B", "1").Error.Code);
    }
}
