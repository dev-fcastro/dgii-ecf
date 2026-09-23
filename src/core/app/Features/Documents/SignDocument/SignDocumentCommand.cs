namespace DgiiEcf.Application.Features.Documents.SignDocument;

/// <summary>
/// Signs an XML document (ECF, RFCE, ARECF, ACECF, ANECF, SemillaModel or any other root).
/// </summary>
public sealed record SignDocumentCommand(string Xml, string? RootElementName = null);
