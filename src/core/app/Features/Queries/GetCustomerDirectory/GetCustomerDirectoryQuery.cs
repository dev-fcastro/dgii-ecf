namespace DgiiEcf.Application.Features.Queries.GetCustomerDirectory;

/// <summary>
/// Reception and approval URLs of an electronic taxpayer (<c>consultadirectorio</c>). Always a list.
/// </summary>
public sealed record GetCustomerDirectoryQuery(string Rnc);
