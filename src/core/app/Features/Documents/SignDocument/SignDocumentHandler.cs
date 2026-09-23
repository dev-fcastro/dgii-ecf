using DgiiEcf.Application.Common.Contracts.IXmlDocumentSigner;
using DgiiEcf.Application.Common.Xml.DgiiXmlSerializer;
using DgiiEcf.Application.Common.Xml.XmlErrors;
using DgiiEcf.Application.Features.Documents.SignDocument.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Documents.SignDocument;

public sealed class SignDocumentHandler : ISignDocumentHandler
{
    private readonly IXmlDocumentSigner _signer;

    public SignDocumentHandler(IXmlDocumentSigner signer)
    {
        _signer = signer;
    }

    public Result<string> Handle(SignDocumentCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Xml))
        {
            return XmlErrors.Empty;
        }

        return _signer.Sign(command.Xml, command.RootElementName);
    }

    public Result<string> HandleDocument<TDocument>(TDocument document) where TDocument : class
    {
        ArgumentNullException.ThrowIfNull(document);

        var rootName = DgiiXmlSerializer.ResolveRootName(document.GetType());
        if (rootName is null)
        {
            return XmlErrors.MissingRootAttribute;
        }

        return _signer.Sign(DgiiXmlSerializer.Serialize(document), rootName);
    }
}
