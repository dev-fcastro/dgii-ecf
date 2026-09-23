using System.Security.Cryptography.X509Certificates;
using DgiiEcf.Application.DependencyInjection.ApplicationDependencyInjection;
using DgiiEcf.ExternalServices.DependencyInjection.ExternalServicesDependencyInjection;
using DgiiEcf.Facades.DgiiEcfClient;
using DgiiEcf.Facades.EcfReceiver;
using DgiiEcf.Signing.DependencyInjection.SigningDependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DgiiEcf.DependencyInjection.DgiiEcfDependencyInjection;

public static class DgiiEcfDependencyInjection
{
    /// <summary>
    /// Registers the whole package from configuration:
    /// <code>
    /// "DgiiEcf": {
    ///   "Environment": "Test",
    ///   "Certificate": { "CertificatePath": "cert.p12", "CertificatePassword": "..." }
    /// }
    /// </code>
    /// </summary>
    public static IServiceCollection AddDgiiEcf(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDgiiEcfApplication();
        services.AddDgiiEcfSigning(configuration);
        services.AddDgiiEcfExternalServices(configuration);
        return services.AddDgiiEcfFacades();
    }

    /// <summary>
    /// Registers the whole package from code.
    /// </summary>
    public static IServiceCollection AddDgiiEcf(this IServiceCollection services, Action<Options.DgiiEcfOptions.DgiiEcfOptions> configure)
    {
        var options = new Options.DgiiEcfOptions.DgiiEcfOptions();
        configure(options);

        services.AddDgiiEcfApplication();
        services.AddDgiiEcfSigning(signing =>
        {
            signing.CertificatePath = options.CertificatePath;
            signing.CertificateBase64 = options.CertificateBase64;
            signing.CertificatePassword = options.CertificatePassword;
        });
        services.AddDgiiEcfExternalServices(api => Apply(options, api));
        return services.AddDgiiEcfFacades();
    }

    /// <summary>
    /// Registers the whole package with a certificate already opened by the caller.
    /// </summary>
    public static IServiceCollection AddDgiiEcf(
        this IServiceCollection services,
        X509Certificate2 certificate,
        Action<Options.DgiiEcfOptions.DgiiEcfOptions>? configure = null)
    {
        var options = new Options.DgiiEcfOptions.DgiiEcfOptions();
        configure?.Invoke(options);

        services.AddDgiiEcfApplication();
        services.AddDgiiEcfSigning(certificate);
        services.AddDgiiEcfExternalServices(api => Apply(options, api));
        return services.AddDgiiEcfFacades();
    }

    private static IServiceCollection AddDgiiEcfFacades(this IServiceCollection services)
    {
        services.TryAddScoped<IDgiiEcfClient, DgiiEcfClient>();
        services.TryAddScoped<IEcfReceiver, EcfReceiver>();
        return services;
    }

    private static void Apply(Options.DgiiEcfOptions.DgiiEcfOptions source, ExternalServices.Options.DgiiApiOptions.DgiiApiOptions target)
    {
        target.Environment = source.Environment;
        target.EcfBaseUrl = source.EcfBaseUrl;
        target.FcBaseUrl = source.FcBaseUrl;
        target.StatusBaseUrl = source.StatusBaseUrl;
        target.StatusApiKey = source.StatusApiKey;
        target.Timeout = source.Timeout;
    }
}
