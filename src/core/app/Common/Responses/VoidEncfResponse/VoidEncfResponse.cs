namespace DgiiEcf.Application.Common.Responses.VoidEncfResponse;

/// <summary>
/// Response of <c>anulacionrangos/api/operaciones/anularrango</c>.
/// </summary>
public sealed record VoidEncfResponse(string? Rnc, string? Codigo, string? Nombre, IReadOnlyList<string>? Mensajes);
