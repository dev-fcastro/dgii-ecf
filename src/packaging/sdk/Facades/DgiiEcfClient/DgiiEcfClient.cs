using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Application.Common.Responses.CommercialApprovalResponse;
using DgiiEcf.Application.Common.Responses.CustomerDirectoryEntry;
using DgiiEcf.Application.Common.Responses.InquiryStatusResponse;
using DgiiEcf.Application.Common.Responses.InvoiceResponse;
using DgiiEcf.Application.Common.Responses.InvoiceSummaryResponse;
using DgiiEcf.Application.Common.Responses.ServiceStatus;
using DgiiEcf.Application.Common.Responses.SummaryInvoiceInquiryResponse;
using DgiiEcf.Application.Common.Responses.TrackIdSummary;
using DgiiEcf.Application.Common.Responses.TrackingStatusResponse;
using DgiiEcf.Application.Common.Responses.VoidEncfResponse;
using DgiiEcf.Application.Features.Authentication.Authenticate;
using DgiiEcf.Application.Features.Authentication.Authenticate.Contracts;
using DgiiEcf.Application.Features.CommercialApproval.SendCommercialApproval;
using DgiiEcf.Application.Features.CommercialApproval.SendCommercialApproval.Contracts;
using DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce;
using DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce.Contracts;
using DgiiEcf.Application.Features.Documents.SignDocument;
using DgiiEcf.Application.Features.Documents.SignDocument.Contracts;
using DgiiEcf.Application.Features.Queries.GetCustomerDirectory;
using DgiiEcf.Application.Features.Queries.GetCustomerDirectory.Contracts;
using DgiiEcf.Application.Features.Queries.GetSummaryInvoiceInquiry;
using DgiiEcf.Application.Features.Queries.GetSummaryInvoiceInquiry.Contracts;
using DgiiEcf.Application.Features.Queries.GetTrackIds;
using DgiiEcf.Application.Features.Queries.GetTrackIds.Contracts;
using DgiiEcf.Application.Features.Queries.GetTrackStatus;
using DgiiEcf.Application.Features.Queries.GetTrackStatus.Contracts;
using DgiiEcf.Application.Features.Queries.InquiryStatus;
using DgiiEcf.Application.Features.Queries.InquiryStatus.Contracts;
using DgiiEcf.Application.Features.Reception.SendElectronicDocument;
using DgiiEcf.Application.Features.Reception.SendElectronicDocument.Contracts;
using DgiiEcf.Application.Features.Reception.SendSummary;
using DgiiEcf.Application.Features.Reception.SendSummary.Contracts;
using DgiiEcf.Application.Features.ServiceStatus.GetMaintenanceWindows;
using DgiiEcf.Application.Features.ServiceStatus.GetMaintenanceWindows.Contracts;
using DgiiEcf.Application.Features.ServiceStatus.GetServicesStatus;
using DgiiEcf.Application.Features.ServiceStatus.GetServicesStatus.Contracts;
using DgiiEcf.Application.Features.ServiceStatus.VerifyServiceStatus;
using DgiiEcf.Application.Features.ServiceStatus.VerifyServiceStatus.Contracts;
using DgiiEcf.Application.Features.Voiding.VoidEncf;
using DgiiEcf.Application.Features.Voiding.VoidEncf.Contracts;
using DgiiEcf.DependencyInjection.DgiiEcfDependencyInjection;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.ExternalServices.Options.DgiiApiOptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DgiiEcf.Facades.DgiiEcfClient;

public sealed class DgiiEcfClient : IDgiiEcfClient
{
    private readonly IServiceProvider _services;
    private readonly DgiiApiOptions _options;

    public DgiiEcfClient(IServiceProvider services, IOptions<DgiiApiOptions> options)
    {
        _services = services;
        _options = options.Value;
    }

    /// <summary>
    /// Creates a standalone client (no host / DI container needed). Dispose the returned scope when done.
    /// </summary>
    public static StandaloneDgiiEcfClient Create(Action<Options.DgiiEcfOptions.DgiiEcfOptions> configure)
    {
        var services = new ServiceCollection();
        services.AddDgiiEcf(configure);
        return new StandaloneDgiiEcfClient(services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true }));
    }

    public Task<Result<AccessToken>> AuthenticateAsync(string? buyerHost = null, CancellationToken cancellationToken = default) =>
        Get<IAuthenticateHandler>().HandleAsync(new AuthenticateCommand(buyerHost), cancellationToken);

    public Result<string> Sign(string xml, string? rootElementName = null) =>
        Get<ISignDocumentHandler>().Handle(new SignDocumentCommand(xml, rootElementName));

    public Result<string> SignDocument<TDocument>(TDocument document) where TDocument : class =>
        Get<ISignDocumentHandler>().HandleDocument(document);

    public Result<SignedRfce> CreateSignedRfce(string signedEcf32Xml)
    {
        var conversion = Get<IConvertEcf32ToRfceHandler>().Handle(new ConvertEcf32ToRfceCommand(signedEcf32Xml));
        if (conversion.IsFailure)
        {
            return conversion.Error;
        }

        var signed = Sign(conversion.Value.Xml, "RFCE");
        return signed.IsSuccess ? new SignedRfce(signed.Value, conversion.Value.SecurityCode, conversion.Value) : signed.Error;
    }

    public Task<Result<InvoiceResponse>> SendElectronicDocumentAsync(
        string signedXml, string? fileName = null, string? buyerHost = null, CancellationToken cancellationToken = default) =>
        Get<ISendElectronicDocumentHandler>().HandleAsync(new SendElectronicDocumentCommand(signedXml, fileName, buyerHost), cancellationToken);

    public Task<Result<InvoiceSummaryResponse>> SendSummaryAsync(string signedRfceXml, string? fileName = null, CancellationToken cancellationToken = default) =>
        Get<ISendSummaryHandler>().HandleAsync(new SendSummaryCommand(signedRfceXml, fileName), cancellationToken);

    public Task<Result<CommercialApprovalResponse>> SendCommercialApprovalAsync(
        string signedAcecfXml, string? fileName = null, string? buyerHost = null, CancellationToken cancellationToken = default) =>
        Get<ISendCommercialApprovalHandler>().HandleAsync(new SendCommercialApprovalCommand(signedAcecfXml, fileName, buyerHost), cancellationToken);

    public Task<Result<VoidEncfResponse>> VoidEncfAsync(string signedAnecfXml, string? fileName = null, CancellationToken cancellationToken = default) =>
        Get<IVoidEncfHandler>().HandleAsync(new VoidEncfCommand(signedAnecfXml, fileName), cancellationToken);

    public Task<Result<TrackingStatusResponse>> GetTrackStatusAsync(string trackId, CancellationToken cancellationToken = default) =>
        Get<IGetTrackStatusHandler>().HandleAsync(new GetTrackStatusQuery(trackId), cancellationToken);

    public Task<Result<InquiryStatusResponse>> InquiryStatusAsync(
        string rncEmisor, string encf, string? rncComprador = null, string? securityCode = null, CancellationToken cancellationToken = default) =>
        Get<IInquiryStatusHandler>().HandleAsync(new InquiryStatusQuery(rncEmisor, encf, rncComprador, securityCode), cancellationToken);

    public Task<Result<IReadOnlyList<TrackIdSummary>>> GetTrackIdsAsync(string rncEmisor, string encf, CancellationToken cancellationToken = default) =>
        Get<IGetTrackIdsHandler>().HandleAsync(new GetTrackIdsQuery(rncEmisor, encf), cancellationToken);

    public Task<Result<IReadOnlyList<CustomerDirectoryEntry>>> GetCustomerDirectoryAsync(string rnc, CancellationToken cancellationToken = default) =>
        Get<IGetCustomerDirectoryHandler>().HandleAsync(new GetCustomerDirectoryQuery(rnc), cancellationToken);

    public Task<Result<SummaryInvoiceInquiryResponse>> GetSummaryInvoiceInquiryAsync(
        string rncEmisor, string encf, string securityCode, CancellationToken cancellationToken = default) =>
        Get<IGetSummaryInvoiceInquiryHandler>().HandleAsync(new GetSummaryInvoiceInquiryQuery(rncEmisor, encf, securityCode), cancellationToken);

    public Task<Result<IReadOnlyList<DgiiServiceStatus>>> GetServicesStatusAsync(string? apiKey = null, CancellationToken cancellationToken = default) =>
        Get<IGetServicesStatusHandler>().HandleAsync(new GetServicesStatusQuery(ApiKey(apiKey)), cancellationToken);

    public Task<Result<MaintenanceResponse>> GetMaintenanceWindowsAsync(string? apiKey = null, CancellationToken cancellationToken = default) =>
        Get<IGetMaintenanceWindowsHandler>().HandleAsync(new GetMaintenanceWindowsQuery(ApiKey(apiKey)), cancellationToken);

    public Task<Result<VerificationResponse>> VerifyServiceStatusAsync(string? apiKey = null, CancellationToken cancellationToken = default) =>
        Get<IVerifyServiceStatusHandler>().HandleAsync(new VerifyServiceStatusQuery(ApiKey(apiKey)), cancellationToken);

    private string ApiKey(string? apiKey) => apiKey ?? _options.StatusApiKey ?? string.Empty;

    private T Get<T>() where T : notnull => _services.GetRequiredService<T>();
}

/// <summary>
/// <see cref="DgiiEcfClient"/> that owns its own service provider (see <see cref="DgiiEcfClient.Create"/>).
/// </summary>
public sealed class StandaloneDgiiEcfClient : IDisposable
{
    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;

    internal StandaloneDgiiEcfClient(ServiceProvider root)
    {
        _root = root;
        _scope = root.CreateScope();
        Client = _scope.ServiceProvider.GetRequiredService<IDgiiEcfClient>();
        Receiver = _scope.ServiceProvider.GetRequiredService<EcfReceiver.IEcfReceiver>();
    }

    public IDgiiEcfClient Client { get; }

    public EcfReceiver.IEcfReceiver Receiver { get; }

    public void Dispose()
    {
        _scope.Dispose();
        _root.Dispose();
    }
}
