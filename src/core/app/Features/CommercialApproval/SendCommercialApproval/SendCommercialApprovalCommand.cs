namespace DgiiEcf.Application.Features.CommercialApproval.SendCommercialApproval;

/// <summary>
/// Sends a signed ACECF to DGII (<c>aprobacionComercial/api/AprobacionComercial</c>) or, with <paramref name="BuyerHost"/>, to the issuer's receiver (<c>fe/aprobacioncomercial/api/ecf</c>).
/// </summary>
public sealed record SendCommercialApprovalCommand(string SignedXml, string? FileName = null, string? BuyerHost = null);
