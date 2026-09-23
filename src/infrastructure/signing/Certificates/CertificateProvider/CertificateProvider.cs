using System.Security.Cryptography.X509Certificates;
using DgiiEcf.Application.Common.Contracts.ICertificateProvider;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.Signing.Options.SigningOptions;
using Microsoft.Extensions.Options;

namespace DgiiEcf.Signing.Certificates.CertificateProvider;

/// <summary>
/// Opens the configured certificate once and keeps it for the lifetime of the container.
/// </summary>
public sealed class CertificateProvider : ICertificateProvider, IDisposable
{
    private readonly Lazy<Result<X509Certificate2>> _certificate;

    public CertificateProvider(IOptions<SigningOptions> options)
    {
        _certificate = new Lazy<Result<X509Certificate2>>(() => Load(options.Value), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Result<X509Certificate2> GetCertificate() => _certificate.Value;

    public void Dispose()
    {
        if (_certificate.IsValueCreated && _certificate.Value.IsSuccess)
        {
            _certificate.Value.Value.Dispose();
        }
    }

    private static Result<X509Certificate2> Load(SigningOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.CertificatePath))
        {
            return CertificateLoader.CertificateLoader.LoadFromFile(options.CertificatePath, options.CertificatePassword);
        }

        if (!string.IsNullOrWhiteSpace(options.CertificateBase64))
        {
            return CertificateLoader.CertificateLoader.LoadFromBase64(options.CertificateBase64, options.CertificatePassword);
        }

        return CertificateErrors.CertificateErrors.NotConfigured;
    }
}

/// <summary>
/// Provider for a certificate already opened by the caller. The caller keeps ownership.
/// </summary>
public sealed class StaticCertificateProvider : ICertificateProvider
{
    private readonly X509Certificate2 _certificate;

    public StaticCertificateProvider(X509Certificate2 certificate)
    {
        _certificate = certificate;
    }

    public Result<X509Certificate2> GetCertificate() => _certificate;
}
