using System.Security.Cryptography.X509Certificates;
using DgiiEcf.Application.Common.Contracts.ICertificateProvider;
using DgiiEcf.Application.Common.Contracts.IJwtTokenService;
using DgiiEcf.Application.Common.Contracts.ISignatureVerifier;
using DgiiEcf.Application.Common.Contracts.IXmlDocumentSigner;
using DgiiEcf.Signing.Certificates.CertificateProvider;
using DgiiEcf.Signing.Jwt.JwtTokenService;
using DgiiEcf.Signing.Options.SigningOptions;
using DgiiEcf.Signing.XmlSigning.SignatureVerifier;
using DgiiEcf.Signing.XmlSigning.XmlDocumentSigner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DgiiEcf.Signing.DependencyInjection.SigningDependencyInjection;

public static class SigningDependencyInjection
{
    /// <summary>
    /// Registers signing services with the certificate bound from <c>DgiiEcf:Certificate</c>.
    /// </summary>
    public static IServiceCollection AddDgiiEcfSigning(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SigningOptions>().Bind(configuration.GetSection(SigningOptions.SectionName)).ValidateOnStart();
        services.AddSingleton<IValidateOptions<SigningOptions>, SigningOptionsValidator>();
        services.TryAddSingleton<ICertificateProvider, CertificateProvider>();
        return services.AddDgiiEcfSigningCore();
    }

    public static IServiceCollection AddDgiiEcfSigning(this IServiceCollection services, Action<SigningOptions> configure)
    {
        services.AddOptions<SigningOptions>().Configure(configure).ValidateOnStart();
        services.AddSingleton<IValidateOptions<SigningOptions>, SigningOptionsValidator>();
        services.TryAddSingleton<ICertificateProvider, CertificateProvider>();
        return services.AddDgiiEcfSigningCore();
    }

    /// <summary>
    /// Registers signing services with a certificate already opened by the caller.
    /// </summary>
    public static IServiceCollection AddDgiiEcfSigning(this IServiceCollection services, X509Certificate2 certificate)
    {
        services.TryAddSingleton<ICertificateProvider>(new StaticCertificateProvider(certificate));
        return services.AddDgiiEcfSigningCore();
    }

    private static IServiceCollection AddDgiiEcfSigningCore(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IXmlDocumentSigner, XmlDocumentSigner>();
        services.TryAddSingleton<ISignatureVerifier, SignatureVerifier>();
        services.TryAddSingleton<IJwtTokenService, JwtTokenService>();
        return services;
    }
}
