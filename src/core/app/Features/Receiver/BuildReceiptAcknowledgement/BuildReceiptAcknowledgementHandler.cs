using DgiiEcf.Application.Common.Time.DominicanClock;
using DgiiEcf.Application.Common.Xml.DgiiXmlSerializer;
using DgiiEcf.Application.Common.Xml.XmlDocumentLoader;
using DgiiEcf.Application.Features.Receiver.BuildReceiptAcknowledgement.Contracts;
using DgiiEcf.Application.Features.Receiver.Common;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.Domain.Documents.Arecf;
using DgiiEcf.Domain.Reception.NoReceivedCode;
using DgiiEcf.Domain.Reception.ReceivedStatus;
using DgiiEcf.Domain.Reception.ReceptionRules;

namespace DgiiEcf.Application.Features.Receiver.BuildReceiptAcknowledgement;

public sealed class BuildReceiptAcknowledgementHandler : IBuildReceiptAcknowledgementHandler
{
    private readonly TimeProvider _timeProvider;

    public BuildReceiptAcknowledgementHandler(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Result<ReceiptAcknowledgement> Handle(BuildReceiptAcknowledgementCommand command)
    {
        var loaded = XmlDocumentLoader.Load(command.ReceivedXml);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        string? Read(string name) => XmlDocumentLoader.FindFirst(loaded.Value, name)?.Value.Trim();

        var encf = Read("eNCF");
        var tipo = Read("TipoeCF");
        var rncEmisor = Read("RNCEmisor");
        var rncComprador = Read("RNCComprador");

        foreach (var (name, value) in new[] { ("eNCF", encf), ("RNCEmisor", rncEmisor), ("RNCComprador", rncComprador) })
        {
            if (value is null)
            {
                return ReceiverErrors.MissingElement(name);
            }
        }

        var status = command.Status;
        var code = command.Code;

        if (ReceptionRules.IsExcludedType(tipo))
        {
            status = ReceivedStatus.NotReceived;
            code = NoReceivedCode.SpecificationError;
        }

        if (!string.Equals(command.ReceiverRnc?.Trim(), rncComprador, StringComparison.Ordinal))
        {
            status = ReceivedStatus.NotReceived;
            code = NoReceivedCode.BuyerRncMismatch;
        }

        var document = new Arecf
        {
            DetalleAcusedeRecibo = new DetalleAcusedeRecibo
            {
                RNCEmisor = rncEmisor,
                RNCComprador = rncComprador,
                Encf = encf,
                Estado = (int)status,
                CodigoMotivoNoRecibido = status == ReceivedStatus.NotReceived ? (int?)code : null,
                FechaHoraAcuseRecibo = DominicanClock.Now(_timeProvider).DateTime,
            },
        };

        return new ReceiptAcknowledgement(DgiiXmlSerializer.Serialize(document), document);
    }
}
