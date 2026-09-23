using System.Xml.Linq;
using DgiiEcf.Application.Features.Receiver.BuildReceiptAcknowledgement;
using DgiiEcf.Application.Tests.Support;
using DgiiEcf.Domain.Reception.NoReceivedCode;
using DgiiEcf.Domain.Reception.ReceivedStatus;

namespace DgiiEcf.Application.Tests.Features.Receiver.BuildReceiptAcknowledgement;

public sealed class BuildReceiptAcknowledgementHandlerTests
{
    private readonly BuildReceiptAcknowledgementHandler _handler =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 8, 17, 12, 15, 30, TimeSpan.Zero)));

    [Fact]
    public void Handle_DocumentForThisReceiver_IsReceived()
    {
        var result = _handler.Handle(new BuildReceiptAcknowledgementCommand(Samples.Read("invoice_received.xml"), "130862346"));

        Assert.True(result.IsSuccess, result.Error.Message);
        var detail = XDocument.Parse(result.Value.Xml).Root!.Element("DetalleAcusedeRecibo")!;
        Assert.Equal(
            ["Version", "RNCEmisor", "RNCComprador", "eNCF", "Estado", "FechaHoraAcuseRecibo"],
            detail.Elements().Select(element => element.Name.LocalName));
        Assert.Equal("1.0", detail.Element("Version")!.Value);
        Assert.Equal("131880738", detail.Element("RNCEmisor")!.Value);
        Assert.Equal("E310000000007", detail.Element("eNCF")!.Value);
        Assert.Equal("0", detail.Element("Estado")!.Value);
        Assert.Equal("17-08-2026 08:15:30", detail.Element("FechaHoraAcuseRecibo")!.Value);
    }

    [Fact]
    public void Handle_DocumentForAnotherBuyer_IsNotReceivedWithCode4()
    {
        var result = _handler.Handle(new BuildReceiptAcknowledgementCommand(Samples.Read("invoice_received.xml"), "MY_RNC"));

        Assert.Equal((int)ReceivedStatus.NotReceived, result.Value.Document.DetalleAcusedeRecibo!.Estado);
        Assert.Equal((int)NoReceivedCode.BuyerRncMismatch, result.Value.Document.DetalleAcusedeRecibo.CodigoMotivoNoRecibido);
        Assert.Contains("<CodigoMotivoNoRecibido>4</CodigoMotivoNoRecibido>", result.Value.Xml);
    }

    [Fact]
    public void Handle_ExcludedType_IsNotReceivedWithCode1()
    {
        var result = _handler.Handle(new BuildReceiptAcknowledgementCommand(Samples.Read("invoice_received_wrong_type.xml"), "130862346"));

        Assert.Equal((int)ReceivedStatus.NotReceived, result.Value.Document.DetalleAcusedeRecibo!.Estado);
        Assert.Equal((int)NoReceivedCode.SpecificationError, result.Value.Document.DetalleAcusedeRecibo.CodigoMotivoNoRecibido);
    }

    [Fact]
    public void Handle_ExplicitDuplicate_KeepsTheGivenCode()
    {
        var result = _handler.Handle(new BuildReceiptAcknowledgementCommand(
            Samples.Read("invoice_received.xml"), "130862346", ReceivedStatus.NotReceived, NoReceivedCode.DuplicatedSubmission));

        Assert.Equal(3, result.Value.Document.DetalleAcusedeRecibo!.CodigoMotivoNoRecibido);
    }

    [Fact]
    public void Handle_WithoutBuyer_ReturnsMissingElement()
    {
        var result = _handler.Handle(new BuildReceiptAcknowledgementCommand("<ECF><eNCF>E310000000001</eNCF><RNCEmisor>1</RNCEmisor></ECF>", "1"));

        Assert.Equal("receiver.missing_element", result.Error.Code);
    }
}
