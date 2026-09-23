using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Receiver.ValidateSignedSeed.Contracts;

public interface IValidateSignedSeedHandler
{
    /// <summary>
    /// Returns the token in the same shape DGII answers (<c>token</c>, <c>expira</c>, <c>expedido</c>).
    /// </summary>
    Result<AccessToken> Handle(ValidateSignedSeedCommand command);
}
