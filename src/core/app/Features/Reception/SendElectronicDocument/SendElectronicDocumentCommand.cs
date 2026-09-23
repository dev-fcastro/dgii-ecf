namespace DgiiEcf.Application.Features.Reception.SendElectronicDocument;

/// <summary>
/// Sends a signed e-CF to DGII (<c>recepcion/api/FacturasElectronicas</c>) or, with <paramref name="BuyerHost"/>, to a receiver (<c>fe/recepcion/api/ecf</c>).
/// </summary>
public sealed record SendElectronicDocumentCommand(string SignedXml, string? FileName = null, string? BuyerHost = null);
