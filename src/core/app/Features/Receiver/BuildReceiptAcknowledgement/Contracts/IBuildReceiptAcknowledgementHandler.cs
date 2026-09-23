using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Receiver.BuildReceiptAcknowledgement.Contracts;

public interface IBuildReceiptAcknowledgementHandler
{
    Result<ReceiptAcknowledgement> Handle(BuildReceiptAcknowledgementCommand command);
}
