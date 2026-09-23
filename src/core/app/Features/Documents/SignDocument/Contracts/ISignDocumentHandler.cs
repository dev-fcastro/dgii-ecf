using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Documents.SignDocument.Contracts;

public interface ISignDocumentHandler
{
    Result<string> Handle(SignDocumentCommand command);

    /// <summary>
    /// Serializes a typed document (<c>DgiiEcf.Domain.Documents.*</c>) and signs it.
    /// </summary>
    Result<string> Handle<TDocument>(TDocument document) where TDocument : class;
}
