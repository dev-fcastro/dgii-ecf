using DgiiEcf.Domain.Documents.Arecf;
using DgiiEcf.Domain.Reception.NoReceivedCode;
using DgiiEcf.Domain.Reception.ReceivedStatus;

namespace DgiiEcf.Application.Features.Receiver.BuildReceiptAcknowledgement;

/// <summary>
/// Builds the unsigned acknowledgement (ARECF) for an e-CF received at <c>fe/recepcion/api/ecf</c>.
/// Excluded types (32, 41, 43, 45, 46, 47) become "No Recibido" with code 1, and a buyer RNC that is not
/// <paramref name="ReceiverRnc"/> becomes "No Recibido" with code 4 (this rule wins).
/// Sign the result with root <c>ARECF</c> before answering.
/// </summary>
public sealed record BuildReceiptAcknowledgementCommand(
    string ReceivedXml,
    string ReceiverRnc,
    ReceivedStatus Status = ReceivedStatus.Received,
    NoReceivedCode? Code = null);

public sealed record ReceiptAcknowledgement(string Xml, Arecf Document);
