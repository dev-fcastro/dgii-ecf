using DgiiEcf.Application.Features.Authentication.AccessTokenProvider;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.Authentication.Authenticate;
using DgiiEcf.Application.Features.Authentication.Authenticate.Contracts;
using DgiiEcf.Application.Features.CommercialApproval.SendCommercialApproval;
using DgiiEcf.Application.Features.CommercialApproval.SendCommercialApproval.Contracts;
using DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce;
using DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce.Contracts;
using DgiiEcf.Application.Features.Documents.GenerateQrCodeUrl;
using DgiiEcf.Application.Features.Documents.GenerateQrCodeUrl.Contracts;
using DgiiEcf.Application.Features.Documents.GetSecurityCode;
using DgiiEcf.Application.Features.Documents.GetSecurityCode.Contracts;
using DgiiEcf.Application.Features.Documents.SignDocument;
using DgiiEcf.Application.Features.Documents.SignDocument.Contracts;
using DgiiEcf.Application.Features.Documents.ValidateDocumentSignature;
using DgiiEcf.Application.Features.Documents.ValidateDocumentSignature.Contracts;
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
using DgiiEcf.Application.Features.Receiver.BuildReceiptAcknowledgement;
using DgiiEcf.Application.Features.Receiver.BuildReceiptAcknowledgement.Contracts;
using DgiiEcf.Application.Features.Receiver.GenerateSeed;
using DgiiEcf.Application.Features.Receiver.GenerateSeed.Contracts;
using DgiiEcf.Application.Features.Receiver.ParseReceivedDocument;
using DgiiEcf.Application.Features.Receiver.ParseReceivedDocument.Contracts;
using DgiiEcf.Application.Features.Receiver.ValidateSignedSeed;
using DgiiEcf.Application.Features.Receiver.ValidateSignedSeed.Contracts;
using DgiiEcf.Application.Features.Receiver.ValidateToken;
using DgiiEcf.Application.Features.Receiver.ValidateToken.Contracts;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DgiiEcf.Application.DependencyInjection.ApplicationDependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddDgiiEcfApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IAuthenticateHandler, AuthenticateHandler>();
        services.AddScoped<IAccessTokenProvider, AccessTokenProvider>();

        services.AddScoped<ISendElectronicDocumentHandler, SendElectronicDocumentHandler>();
        services.AddScoped<ISendSummaryHandler, SendSummaryHandler>();
        services.AddScoped<ISendCommercialApprovalHandler, SendCommercialApprovalHandler>();
        services.AddScoped<IVoidEncfHandler, VoidEncfHandler>();

        services.AddScoped<IGetTrackStatusHandler, GetTrackStatusHandler>();
        services.AddScoped<IInquiryStatusHandler, InquiryStatusHandler>();
        services.AddScoped<IGetTrackIdsHandler, GetTrackIdsHandler>();
        services.AddScoped<IGetCustomerDirectoryHandler, GetCustomerDirectoryHandler>();
        services.AddScoped<IGetSummaryInvoiceInquiryHandler, GetSummaryInvoiceInquiryHandler>();

        services.AddScoped<IGetServicesStatusHandler, GetServicesStatusHandler>();
        services.AddScoped<IGetMaintenanceWindowsHandler, GetMaintenanceWindowsHandler>();
        services.AddScoped<IVerifyServiceStatusHandler, VerifyServiceStatusHandler>();

        services.AddScoped<ISignDocumentHandler, SignDocumentHandler>();
        services.AddScoped<IValidateDocumentSignatureHandler, ValidateDocumentSignatureHandler>();
        services.AddSingleton<IGetSecurityCodeHandler, GetSecurityCodeHandler>();
        services.AddSingleton<IConvertEcf32ToRfceHandler, ConvertEcf32ToRfceHandler>();
        services.AddSingleton<IGenerateQrCodeUrlHandler, GenerateQrCodeUrlHandler>();

        services.AddScoped<IGenerateSeedHandler, GenerateSeedHandler>();
        services.AddScoped<IValidateSignedSeedHandler, ValidateSignedSeedHandler>();
        services.AddScoped<IValidateTokenHandler, ValidateTokenHandler>();
        services.AddScoped<IBuildReceiptAcknowledgementHandler, BuildReceiptAcknowledgementHandler>();
        services.AddSingleton<IParseReceivedDocumentHandler, ParseReceivedDocumentHandler>();

        return services;
    }
}
