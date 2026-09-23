namespace DgiiEcf.Application.Features.Receiver.ParseReceivedDocument;

/// <summary>
/// Extracts the XML file from a <c>multipart/form-data</c> body sent by an issuer. Set
/// <paramref name="IsBase64Encoded"/> when the body arrives base64 encoded (e.g. API Gateway / Lambda).
/// </summary>
public sealed record ParseReceivedDocumentCommand(string Body, string ContentType, bool IsBase64Encoded = false);

public sealed record ReceivedDocument(string FileName, string XmlContent);
