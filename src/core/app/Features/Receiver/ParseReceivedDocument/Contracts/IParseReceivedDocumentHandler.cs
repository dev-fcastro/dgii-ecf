using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Receiver.ParseReceivedDocument.Contracts;

public interface IParseReceivedDocumentHandler
{
    Result<ReceivedDocument> Handle(ParseReceivedDocumentCommand command);

    /// <summary>
    /// Same as <see cref="Handle(ParseReceivedDocumentCommand)"/> for a raw body already read as bytes.
    /// </summary>
    Result<ReceivedDocument> Handle(ReadOnlySpan<byte> body, string contentType);
}
