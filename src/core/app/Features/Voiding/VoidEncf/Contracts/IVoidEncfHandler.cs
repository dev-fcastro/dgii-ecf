using DgiiEcf.Application.Common.Responses.VoidEncfResponse;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Voiding.VoidEncf.Contracts;

public interface IVoidEncfHandler
{
    Task<Result<VoidEncfResponse>> HandleAsync(VoidEncfCommand command, CancellationToken cancellationToken = default);
}
