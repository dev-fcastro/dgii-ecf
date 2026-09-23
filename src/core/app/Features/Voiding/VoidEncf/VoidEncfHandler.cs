using DgiiEcf.Application.Common.Contracts.IDgiiReceptionClient;
using DgiiEcf.Application.Common.Responses.VoidEncfResponse;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.Reception.SignedDocumentGuard;
using DgiiEcf.Application.Features.Voiding.VoidEncf.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Voiding.VoidEncf;

public sealed class VoidEncfHandler : IVoidEncfHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly IDgiiReceptionClient _client;

    public VoidEncfHandler(IAccessTokenProvider tokenProvider, IDgiiReceptionClient client)
    {
        _tokenProvider = tokenProvider;
        _client = client;
    }

    public async Task<Result<VoidEncfResponse>> HandleAsync(VoidEncfCommand command, CancellationToken cancellationToken = default)
    {
        var fileName = SignedDocumentGuard.Check(command.SignedXml, command.FileName);
        if (fileName.IsFailure)
        {
            return fileName.Error;
        }

        return await _tokenProvider.ExecuteAsync(
            null,
            (token, ct) => _client.VoidEncfAsync(command.SignedXml, fileName.Value, token, ct),
            cancellationToken);
    }
}
