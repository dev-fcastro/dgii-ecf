using DgiiEcf.Domain.Common.Results;
using DgiiEcf.Domain.Documents.SecurityCode;

namespace DgiiEcf.Application.Features.Documents.GetSecurityCode.Contracts;

public interface IGetSecurityCodeHandler
{
    Result<SecurityCode> Handle(GetSecurityCodeQuery query);
}
