using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce.Contracts;

public interface IConvertEcf32ToRfceHandler
{
    Result<RfceConversion> Handle(ConvertEcf32ToRfceCommand command);
}
