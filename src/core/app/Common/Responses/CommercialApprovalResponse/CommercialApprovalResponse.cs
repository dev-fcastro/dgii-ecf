namespace DgiiEcf.Application.Common.Responses.CommercialApprovalResponse;

/// <summary>
/// Response of <c>aprobacionComercial/api/AprobacionComercial</c>. <c>codigo</c>: "01" aprobada, "02" rechazada.
/// </summary>
public sealed record CommercialApprovalResponse(string? Codigo, string? Estado, IReadOnlyList<string>? Mensaje);
