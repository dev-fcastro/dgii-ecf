using DgiiEcf.Application.Common.Contracts.IDgiiReceptionClient;
using DgiiEcf.Application.Common.Responses.InvoiceSummaryResponse;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.Reception.SendSummary.Contracts;
using DgiiEcf.Application.Features.Reception.SignedDocumentGuard;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Reception.SendSummary;

public sealed class SendSummaryHandler : ISendSummaryHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly IDgiiReceptionClient _client;

    public SendSummaryHandler(IAccessTokenProvider tokenProvider, IDgiiReceptionClient client)
    {
        _tokenProvider = tokenProvider;
        _client = client;
    }

    public async Task<Result<InvoiceSummaryResponse>> HandleAsync(SendSummaryCommand command, CancellationToken cancellationToken = default)
    {
        var fileName = SignedDocumentGuard.Check(command.SignedXml, command.FileName);
        if (fileName.IsFailure)
        {
            return fileName.Error;
        }

        return await _tokenProvider.ExecuteAsync(
            null,
            (token, ct) => _client.SendSummaryAsync(command.SignedXml, fileName.Value, token, ct),
            cancellationToken);
    }
}
