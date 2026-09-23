using DgiiEcf.Application.Common.Responses.InvoiceResponse;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Reception.SendElectronicDocument.Contracts;

public interface ISendElectronicDocumentHandler
{
    Task<Result<InvoiceResponse>> HandleAsync(SendElectronicDocumentCommand command, CancellationToken cancellationToken = default);
}
