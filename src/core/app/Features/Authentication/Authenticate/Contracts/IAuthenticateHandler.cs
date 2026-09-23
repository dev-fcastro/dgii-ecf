using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Authentication.Authenticate.Contracts;

public interface IAuthenticateHandler
{
    Task<Result<AccessToken>> HandleAsync(AuthenticateCommand command, CancellationToken cancellationToken = default);
}
