namespace DgiiEcf.Application.Common.Responses.CustomerDirectoryEntry;

/// <summary>
/// Entry of the electronic taxpayers directory (<c>consultadirectorio</c>).
/// </summary>
public sealed record CustomerDirectoryEntry(
    string? Nombre,
    string? Rnc,
    string? UrlRecepcion,
    string? UrlAceptacion,
    string? UrlOpcional);
