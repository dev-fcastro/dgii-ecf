namespace DgiiEcf.Application.Features.Voiding.VoidEncf;

/// <summary>
/// Sends a signed ANECF to void unused e-NCF ranges (<c>anulacionrangos/api/operaciones/anularrango</c>).
/// </summary>
public sealed record VoidEncfCommand(string SignedXml, string? FileName = null);
