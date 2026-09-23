using System.Net;
using System.Security.Authentication;
using DgiiEcf.Application.Common.Contracts.IAccessTokenStore;
using DgiiEcf.Application.Common.Contracts.IDgiiAuthenticationClient;
using DgiiEcf.Application.Common.Contracts.IDgiiQueryClient;
using DgiiEcf.Application.Common.Contracts.IDgiiReceptionClient;
using DgiiEcf.Application.Common.Contracts.IDgiiServiceStatusClient;
using DgiiEcf.ExternalServices.Authentication.InMemoryAccessTokenStore;
using DgiiEcf.ExternalServices.Clients.DgiiAuthenticationClient;
using DgiiEcf.ExternalServices.Clients.DgiiQueryClient;
using DgiiEcf.ExternalServices.Clients.DgiiReceptionClient;
using DgiiEcf.ExternalServices.Clients.DgiiServiceStatusClient;
using DgiiEcf.ExternalServices.Http.DgiiHttpTransport;
using DgiiEcf.ExternalServices.Options.DgiiApiOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DgiiEcf.ExternalServices.DependencyInjection.ExternalServicesDependencyInjection;

public static class ExternalServicesDependencyInjection
{
    /// <summary>
    /// Registers the DGII HTTP clients bound from the <c>DgiiEcf</c> section.
    /// </summary>
    public static IServiceCollection AddDgiiEcfExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DgiiApiOptions>().Bind(configuration.GetSection(DgiiApiOptions.SectionName)).ValidateOnStart();
        return services.AddDgiiEcfExternalServicesCore();
    }

    public static IServiceCollection AddDgiiEcfExternalServices(this IServiceCollection services, Action<DgiiApiOptions> configure)
    {
        services.AddOptions<DgiiApiOptions>().Configure(configure).ValidateOnStart();
        return services.AddDgiiEcfExternalServicesCore();
    }

    private static IServiceCollection AddDgiiEcfExternalServicesCore(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<DgiiApiOptions>, DgiiApiOptionsValidator>());
        services.TryAddSingleton<IAccessTokenStore, InMemoryAccessTokenStore>();

        services.AddHttpClient<DgiiHttpTransport>((provider, client) =>
            {
                client.Timeout = provider.GetRequiredService<IOptions<DgiiApiOptions>>().Value.Timeout;
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                SslOptions = { EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13 },
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            });

        services.TryAddTransient<IDgiiAuthenticationClient, DgiiAuthenticationClient>();
        services.TryAddTransient<IDgiiReceptionClient, DgiiReceptionClient>();
        services.TryAddTransient<IDgiiQueryClient, DgiiQueryClient>();
        services.TryAddTransient<IDgiiServiceStatusClient, DgiiServiceStatusClient>();

        return services;
    }
}
