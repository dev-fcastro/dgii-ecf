using DgiiEcf.Application.Common.Contracts.IDgiiReceptionClient;
using DgiiEcf.Application.Common.Responses.InvoiceResponse;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.Reception.Common;
using DgiiEcf.Application.Features.Reception.SendElectronicDocument.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Reception.SendElectronicDocument;

public sealed class SendElectronicDocumentHandler : ISendElectronicDocumentHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly IDgiiReceptionClient _client;

    public SendElectronicDocumentHandler(IAccessTokenProvider tokenProvider, IDgiiReceptionClient client)
    {
        _tokenProvider = tokenProvider;
        _client = client;
    }

    public async Task<Result<InvoiceResponse>> HandleAsync(SendElectronicDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var fileName = SignedDocumentGuard.Check(command.SignedXml, command.FileName);
        if (fileName.IsFailure)
        {
            return fileName.Error;
        }

        return await _tokenProvider.ExecuteAsync(
            command.BuyerHost,
            (token, ct) => _client.SendElectronicDocumentAsync(command.SignedXml, fileName.Value, token, command.BuyerHost, ct),
            cancellationToken);
    }
}
