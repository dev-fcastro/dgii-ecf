using System.Text;
using DgiiEcf.Application.Features.Receiver.Common;
using DgiiEcf.Application.Features.Receiver.ParseReceivedDocument;
using DgiiEcf.Application.Tests.Support;

namespace DgiiEcf.Application.Tests.Features.Receiver.ParseReceivedDocument;

public sealed class ParseReceivedDocumentHandlerTests
{
    private const string Boundary = "--------------------------8db9dd560eaa6d1";
    private const string ContentType = "multipart/form-data; boundary=" + Boundary;
    private const string Xml = "<ECF><Encabezado><IdDoc><eNCF>E310000000002</eNCF></IdDoc></Encabezado></ECF>";

    private static readonly string Body =
        $"--{Boundary}\r\n" +
        "Content-Disposition: form-data; name=\"xml\"; filename=\"131880738E310000000002.xml\"\r\n" +
        "Content-Type: application/xml\r\n" +
        "\r\n" +
        Xml + "\r\n" +
        $"--{Boundary}--\r\n";

    private readonly ParseReceivedDocumentHandler _handler = new();

    [Fact]
    public void Handle_MultipartBody_ReturnsFileNameAndXml()
    {
        var result = _handler.Handle(new ParseReceivedDocumentCommand(Body, ContentType));

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.Equal("131880738E310000000002.xml", result.Value.FileName);
        Assert.Equal(Xml, result.Value.XmlContent);
    }

    [Fact]
    public void Handle_Base64Body_DecodesItFirst()
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(Body));

        var result = _handler.Handle(new ParseReceivedDocumentCommand(base64, ContentType, IsBase64Encoded: true));

        Assert.Equal(Xml, result.Value.XmlContent);
    }

    [Fact]
    public void Handle_DgiiMultipartWithUnquotedParameters_ReturnsTheXml()
    {
        var body = Samples.Read("commercial_approval_response.xml");
        var boundary = body[2..body.IndexOf('\n')].TrimEnd('\r');

        var result = _handler.Handle(new ParseReceivedDocumentCommand(body, $"multipart/form-data; boundary=\"{boundary}\""));

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.Equal("aprobacionComercial.xml", result.Value.FileName);
        Assert.StartsWith("<?xml version=\"1.0\"?><ACECF", result.Value.XmlContent);
        Assert.EndsWith("</ACECF>", result.Value.XmlContent);
    }

    [Fact]
    public void Handle_WithoutFilePart_ReturnsIncompleteFormData()
    {
        var body = $"--{Boundary}\r\nContent-Disposition: form-data; name=\"other\"\r\n\r\nvalue\r\n--{Boundary}--\r\n";

        Assert.Equal(ReceiverErrors.IncompleteFormData, _handler.Handle(new ParseReceivedDocumentCommand(body, ContentType)).Error);
    }

    [Fact]
    public void Handle_WithoutBoundary_ReturnsMissingBoundary()
    {
        Assert.Equal(ReceiverErrors.MissingBoundary, _handler.Handle(new ParseReceivedDocumentCommand(Body, "application/xml")).Error);
    }

    [Fact]
    public void Handle_EmptyBody_ReturnsEmptyBody()
    {
        Assert.Equal(ReceiverErrors.EmptyBody, _handler.Handle(new ParseReceivedDocumentCommand(string.Empty, ContentType)).Error);
    }
}
