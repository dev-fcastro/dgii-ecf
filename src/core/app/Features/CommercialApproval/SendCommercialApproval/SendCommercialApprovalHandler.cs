using DgiiEcf.Application.Common.Contracts.IDgiiReceptionClient;
using DgiiEcf.Application.Common.Responses.CommercialApprovalResponse;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.CommercialApproval.SendCommercialApproval.Contracts;
using DgiiEcf.Application.Features.Reception.Common;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.CommercialApproval.SendCommercialApproval;

public sealed class SendCommercialApprovalHandler : ISendCommercialApprovalHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly IDgiiReceptionClient _client;

    public SendCommercialApprovalHandler(IAccessTokenProvider tokenProvider, IDgiiReceptionClient client)
    {
        _tokenProvider = tokenProvider;
        _client = client;
    }

    public async Task<Result<CommercialApprovalResponse>> HandleAsync(SendCommercialApprovalCommand command, CancellationToken cancellationToken = default)
    {
        var fileName = SignedDocumentGuard.Check(command.SignedXml, command.FileName);
        if (fileName.IsFailure)
        {
            return fileName.Error;
        }

        return await _tokenProvider.ExecuteAsync(
            command.BuyerHost,
            (token, ct) => _client.SendCommercialApprovalAsync(command.SignedXml, fileName.Value, token, command.BuyerHost, ct),
            cancellationToken);
    }
}
