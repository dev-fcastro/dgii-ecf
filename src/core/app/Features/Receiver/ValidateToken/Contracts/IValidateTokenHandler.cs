using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Receiver.ValidateToken.Contracts;

public interface IValidateTokenHandler
{
    Result<ReceiverTokenInfo> Handle(ValidateTokenQuery query);
}
