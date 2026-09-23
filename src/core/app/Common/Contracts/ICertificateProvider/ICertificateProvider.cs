using System.Security.Cryptography.X509Certificates;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Contracts.ICertificateProvider;

/// <summary>
/// Supplies the taxpayer digital certificate (.p12) with its private key.
/// </summary>
public interface ICertificateProvider
{
    Result<X509Certificate2> GetCertificate();
}
