using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Application.Features.Documents.SignDocument;
using DgiiEcf.Application.Features.Documents.SignDocument.Contracts;
using DgiiEcf.Application.Features.Receiver.BuildReceiptAcknowledgement;
using DgiiEcf.Application.Features.Receiver.BuildReceiptAcknowledgement.Contracts;
using DgiiEcf.Application.Features.Receiver.GenerateSeed;
using DgiiEcf.Application.Features.Receiver.GenerateSeed.Contracts;
using DgiiEcf.Application.Features.Receiver.ParseReceivedDocument;
using DgiiEcf.Application.Features.Receiver.ParseReceivedDocument.Contracts;
using DgiiEcf.Application.Features.Receiver.ValidateSignedSeed;
using DgiiEcf.Application.Features.Receiver.ValidateSignedSeed.Contracts;
using DgiiEcf.Application.Features.Receiver.ValidateToken;
using DgiiEcf.Application.Features.Receiver.ValidateToken.Contracts;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.Domain.Reception.NoReceivedCode;
using DgiiEcf.Domain.Reception.ReceivedStatus;

namespace DgiiEcf.Facades.EcfReceiver;

/// <summary>
/// Receiver side of the emisor-receptor standard (equivalent to <c>CustomAuthentication</c> and
/// <c>SenderReceiver</c> of the Node package). Use it to implement the <c>fe/...</c> endpoints.
/// </summary>
public interface IEcfReceiver
{
    /// <summary><c>GET fe/autenticacion/api/semilla</c>.</summary>
    string GenerateSeed();

    /// <summary><c>POST fe/autenticacion/api/validacioncertificado</c>: validates the signed seed and issues a token.</summary>
    Result<AccessToken> ValidateSignedSeed(string signedSeedXml);

    /// <summary>Validates the bearer token sent to the other <c>fe/...</c> endpoints.</summary>
    Result<ReceiverTokenInfo> ValidateToken(string token);

    /// <summary>Extracts the XML file from a multipart body.</summary>
    Result<ReceivedDocument> ParseReceivedDocument(string body, string contentType, bool isBase64Encoded = false);

    /// <summary>Builds the unsigned ARECF for a received e-CF.</summary>
    Result<ReceiptAcknowledgement> BuildReceiptAcknowledgement(
        string receivedXml, string receiverRnc, ReceivedStatus status = ReceivedStatus.Received, NoReceivedCode? code = null);

    /// <summary>Builds and signs the ARECF answered by <c>POST fe/recepcion/api/ecf</c>.</summary>
    Result<string> BuildSignedReceiptAcknowledgement(
        string receivedXml, string receiverRnc, ReceivedStatus status = ReceivedStatus.Received, NoReceivedCode? code = null);
}

public sealed class EcfReceiver : IEcfReceiver
{
    private readonly IGenerateSeedHandler _generateSeed;
    private readonly IValidateSignedSeedHandler _validateSignedSeed;
    private readonly IValidateTokenHandler _validateToken;
    private readonly IParseReceivedDocumentHandler _parseReceivedDocument;
    private readonly IBuildReceiptAcknowledgementHandler _buildReceiptAcknowledgement;
    private readonly ISignDocumentHandler _signDocument;

    public EcfReceiver(
        IGenerateSeedHandler generateSeed,
        IValidateSignedSeedHandler validateSignedSeed,
        IValidateTokenHandler validateToken,
        IParseReceivedDocumentHandler parseReceivedDocument,
        IBuildReceiptAcknowledgementHandler buildReceiptAcknowledgement,
        ISignDocumentHandler signDocument)
    {
        _generateSeed = generateSeed;
        _validateSignedSeed = validateSignedSeed;
        _validateToken = validateToken;
        _parseReceivedDocument = parseReceivedDocument;
        _buildReceiptAcknowledgement = buildReceiptAcknowledgement;
        _signDocument = signDocument;
    }

    public string GenerateSeed() => _generateSeed.Handle(new GenerateSeedCommand());

    public Result<AccessToken> ValidateSignedSeed(string signedSeedXml) =>
        _validateSignedSeed.Handle(new ValidateSignedSeedCommand(signedSeedXml));

    public Result<ReceiverTokenInfo> ValidateToken(string token) => _validateToken.Handle(new ValidateTokenQuery(token));

    public Result<ReceivedDocument> ParseReceivedDocument(string body, string contentType, bool isBase64Encoded = false) =>
        _parseReceivedDocument.Handle(new ParseReceivedDocumentCommand(body, contentType, isBase64Encoded));

    public Result<ReceiptAcknowledgement> BuildReceiptAcknowledgement(
        string receivedXml, string receiverRnc, ReceivedStatus status = ReceivedStatus.Received, NoReceivedCode? code = null) =>
        _buildReceiptAcknowledgement.Handle(new BuildReceiptAcknowledgementCommand(receivedXml, receiverRnc, status, code));

    public Result<string> BuildSignedReceiptAcknowledgement(
        string receivedXml, string receiverRnc, ReceivedStatus status = ReceivedStatus.Received, NoReceivedCode? code = null)
    {
        var acknowledgement = BuildReceiptAcknowledgement(receivedXml, receiverRnc, status, code);
        return acknowledgement.IsSuccess
            ? _signDocument.Handle(new SignDocumentCommand(acknowledgement.Value.Xml, "ARECF"))
            : acknowledgement.Error;
    }
}
